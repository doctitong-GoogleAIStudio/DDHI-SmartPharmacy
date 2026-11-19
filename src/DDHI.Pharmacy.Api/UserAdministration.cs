using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DDHI.Pharmacy.Application;
using DDHI.Pharmacy.Contracts;
using DDHI.Pharmacy.Domain;
using DDHI.Pharmacy.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

static class UserAdministration
{
    const string FullNameClaim="ddhi:full_name";
    static void Success(IdentityResult result)=>Safety.Require(result.Succeeded,string.Join("; ",result.Errors.Select(e=>e.Description)));
    static void Confirm(bool confirmed,string reason)=>Safety.Require(confirmed&&!string.IsNullOrWhiteSpace(reason)&&reason.Length<=500,"Confirm the account change and provide a reason (up to 500 characters).");
    static async Task<object> Row(PharmacyUser user,UserManager<PharmacyUser> users)=>new {
        user.Id,user.Email,FullName=(await users.GetClaimsAsync(user)).FirstOrDefault(c=>c.Type==FullNameClaim)?.Value??"",
        PhoneNumber=user.PhoneNumber??"",Roles=await users.GetRolesAsync(user),user.LockoutEnd,Version=user.ConcurrencyStamp
    };
    static async Task<IResult> Change(HttpContext context,InventoryService service,object input,Func<Actor,Task<object>> action)
    {
        var actor=new Actor(context.User.FindFirstValue(ClaimTypes.NameIdentifier)??"",ActorRoles.ForOperation(context.User.FindAll(ClaimTypes.Role).Select(c=>c.Value),context.Request.Path.ToString()),context.Request.Headers["X-Workstation"].ToString(),context.Connection.RemoteIpAddress?.ToString()??"",AuditEndpoints.Session(context));
        var result=await service.Execute(context.Request.Headers["Idempotency-Key"].ToString(),actor,new{Path=context.Request.Path.ToString(),Input=input},async()=>{
            var manager=context.RequestServices.GetRequiredService<UserManager<PharmacyUser>>();
            var current=await manager.Users.AsNoTracking().SingleOrDefaultAsync(u=>u.Id==actor.UserId);
            var signIn=context.RequestServices.GetRequiredService<SignInManager<PharmacyUser>>();
            Safety.Require(current!=null&&!await manager.IsLockedOutAsync(current)&&await manager.IsInRoleAsync(current,Roles.SystemAdmin)&&
                await signIn.ValidateSecurityStampAsync(current,context.User.FindFirstValue(signIn.Options.ClaimsIdentity.SecurityStampClaimType)),"The administrator account changed. Sign in again before changing accounts.");
            return await action(actor);
        });
        return Results.Content(result,"application/json");
    }
    static async Task PreserveAdministrator(PharmacyUser target,UserManager<PharmacyUser> users,bool removesAdmin)
    {
        if(!removesAdmin||!await users.IsInRoleAsync(target,Roles.SystemAdmin)||await users.IsLockedOutAsync(target))return;
        var admins=await users.GetUsersInRoleAsync(Roles.SystemAdmin);var active=0;
        foreach(var admin in admins)if(!await users.IsLockedOutAsync(admin))active++;
        Safety.Require(active>1,"The last active System Administrator cannot be removed, locked, or demoted.");
    }
    public static void Map(RouteGroupBuilder api)
    {
        api.MapPost("/users",async(UserInput input,HttpContext context,InventoryService service,UserManager<PharmacyUser> users)=>await Change(context,service,new{input.Email,input.Role},async actor=>{
            Safety.Require(Roles.All.Contains(input.Role),"Unknown role.");Safety.Require(!string.IsNullOrEmpty(input.Password),"Enter a strong password.");
            var email=input.Email?.Trim()??"";var user=new PharmacyUser{Email=email,UserName=email,EmailConfirmed=true};
            Success(await users.CreateAsync(user,input.Password));Success(await users.AddToRoleAsync(user,input.Role));
            return new{user.Id,user.Email,input.Role};
        })).RequireAuthorization("Admin");
        api.MapGet("/users",async(UserManager<PharmacyUser> users)=>{
            var result=new List<object>();foreach(var user in await users.Users.OrderBy(u=>u.Email).ToListAsync())result.Add(await Row(user,users));return result;
        }).RequireAuthorization("Admin");
        api.MapGet("/users/{id}",async(string id,UserManager<PharmacyUser> users)=>await Row(await users.FindByIdAsync(id)??throw new InvalidOperationException("Account not found."),users)).RequireAuthorization("Admin");
        api.MapPost("/users/{id}/edit",async(string id,UserEditInput input,HttpContext context,InventoryService service,UserManager<PharmacyUser> users)=>await Change(context,service,input,async actor=>{
            Confirm(input.Confirmed,input.Reason);
            var user=await users.FindByIdAsync(id)??throw new InvalidOperationException("Account not found.");
            Safety.Require(!string.IsNullOrEmpty(input.Version)&&input.Version==user.ConcurrencyStamp,"This account changed since you opened it. Reload before editing.");
            var email=input.Email?.Trim()??"";var name=input.FullName?.Trim()??"";var phone=input.PhoneNumber?.Trim()??"";
            Safety.Require(email.Length<=254&&new EmailAddressAttribute().IsValid(email)&&!email.Any(char.IsControl),"Enter a valid staff email.");
            Safety.Require(name.Length<=120&&!name.Any(char.IsControl),"Full name must be at most 120 characters without control characters.");
            Safety.Require(phone.Length<=32&&phone.All(c=>char.IsAsciiDigit(c)||" +()-./".Contains(c)),"Enter a valid optional phone number (up to 32 characters).");
            var roles=(input.Roles??[]).Distinct(StringComparer.Ordinal).Order().ToArray();
            Safety.Require(roles.Length>0&&roles.Length<=Roles.All.Length&&roles.All(Roles.All.Contains),"Select one or more supported roles.");
            var prior=(await users.GetRolesAsync(user)).Order().ToArray();
            context.RequestServices.GetRequiredService<PharmacyDb>().AuditCapture?.Note("Staff account",id,"Assigned roles",string.Join(", ",prior),string.Join(", ",roles));
            Safety.Require(id!=actor.UserId||prior.SequenceEqual(roles),"You cannot change your own roles. Another System Administrator must do that.");
            await PreserveAdministrator(user,users,!roles.Contains(Roles.SystemAdmin));
            var other=await users.FindByEmailAsync(email);Safety.Require(other==null||other.Id==id,"Another account already uses this email.");
            Success(await users.SetEmailAsync(user,email));Success(await users.SetUserNameAsync(user,email));
            user.EmailConfirmed=true;user.PhoneNumber=phone.Length==0?null:phone;user.PhoneNumberConfirmed=false;
            var claims=(await users.GetClaimsAsync(user)).Where(c=>c.Type==FullNameClaim).ToArray();
            if(claims.Length>0)Success(await users.RemoveClaimsAsync(user,claims));
            if(name.Length>0)Success(await users.AddClaimAsync(user,new Claim(FullNameClaim,name)));
            var remove=prior.Except(roles).ToArray();var add=roles.Except(prior).ToArray();
            if(remove.Length>0)Success(await users.RemoveFromRolesAsync(user,remove));if(add.Length>0)Success(await users.AddToRolesAsync(user,add));
            Success(await users.UpdateSecurityStampAsync(user));Success(await users.UpdateAsync(user));
            return await Row(user,users);
        })).RequireAuthorization("Admin");
        api.MapPost("/users/{id}/delete",async(string id,UserDeleteInput input,HttpContext context,InventoryService service,UserManager<PharmacyUser> users)=>await Change(context,service,input,async actor=>{
            Confirm(input.Confirmed,input.Reason);Safety.Require(id!=actor.UserId,"You cannot delete the account you are signed in with.");
            var user=await users.FindByIdAsync(id)??throw new InvalidOperationException("Account not found.");
            Safety.Require(input.Version==user.ConcurrencyStamp,"This account changed since you opened it. Reload before deleting.");
            Safety.Require(users.NormalizeEmail(input.ConfirmationEmail?.Trim()??"")==user.NormalizedEmail,"Type the selected account's email to confirm deletion.");
            await PreserveAdministrator(user,users,true);
            var db=context.RequestServices.GetRequiredService<PharmacyDb>();
            db.AuditCapture?.Note("Staff account",id,"Assigned roles",string.Join(", ",await users.GetRolesAsync(user)),null);
            // Identity relationships are RESTRICT in this database. Remove only account-private
            // children explicitly; clinical/audit/history tables are never deleted here.
            await db.UserClaims.Where(x=>x.UserId==id).ExecuteDeleteAsync();
            await db.UserLogins.Where(x=>x.UserId==id).ExecuteDeleteAsync();
            await db.UserTokens.Where(x=>x.UserId==id).ExecuteDeleteAsync();
            await db.UserRoles.Where(x=>x.UserId==id).ExecuteDeleteAsync();
            Success(await users.DeleteAsync(user));
            return new{Id=id,Deleted=true,HistoryPreserved=true};
        })).RequireAuthorization("Admin");
        api.MapPost("/users/{id}/lock",async(string id,UserLockInput input,HttpContext context,InventoryService service,UserManager<PharmacyUser> users)=>await Change(context,service,input,async actor=>{
            Confirm(input.Confirmed,input.Reason);Safety.Require(id!=actor.UserId,"You cannot lock your own account.");
            var user=await users.FindByIdAsync(id)??throw new InvalidOperationException("Account not found.");
            if(input.Locked)await PreserveAdministrator(user,users,true);
            Success(await users.SetLockoutEnabledAsync(user,true));Success(await users.SetLockoutEndDateAsync(user,input.Locked?DateTimeOffset.UtcNow.AddYears(50):null));
            Success(await users.UpdateSecurityStampAsync(user));return new{user.Id,input.Locked};
        })).RequireAuthorization("Admin");
    }
}

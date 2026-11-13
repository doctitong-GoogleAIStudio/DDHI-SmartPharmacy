using DDHI.Pharmacy.Tools;
using System.Diagnostics;
using System.Security.Cryptography;
if(args.Length!=3||args[0] is not ("backup" or "restore-test")){Console.Error.WriteLine("Usage: DDHI.Pharmacy.Tools backup <pg_dump executable> <new .ddhibak path> OR restore-test <pg_restore executable> <backup path>. Use PGHOST, PGPORT, PGUSER, PGDATABASE, PGPASSWORD and DDHI_BACKUP_PASSWORD environment variables. Restore target must start ddhi_restore_.");return 2;}
var pass=Environment.GetEnvironmentVariable("DDHI_BACKUP_PASSWORD")??throw new InvalidOperationException("DDHI_BACKUP_PASSWORD required.");
if(args[0]=="backup"){
 var final=Path.GetFullPath(args[2]);if(File.Exists(final))throw new IOException("Backup exists; refusing overwrite.");var partial=final+".partial";
 var info=new ProcessStartInfo(args[1]){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(var a in new[]{"--format=custom","--no-password"})info.ArgumentList.Add(a);
 using var process=Process.Start(info)!;var error=process.StandardError.ReadToEndAsync();try{using(var output=new FileStream(partial,FileMode.CreateNew)){BackupCrypt.Encrypt(process.StandardOutput.BaseStream,output,pass);}await process.WaitForExitAsync();if(process.ExitCode!=0)throw new InvalidOperationException("pg_dump failed. Verify backup account, database and server connectivity.");
  // Authenticate the complete encrypted backup before declaring success.
  using(var input=File.OpenRead(partial))BackupCrypt.Decrypt(input,Stream.Null,pass);File.Move(partial,final);using var saved=File.OpenRead(final);File.WriteAllText(final+".sha256",Convert.ToHexString(SHA256.HashData(saved))+"  "+Path.GetFileName(final));Console.WriteLine("Encrypted and authenticated backup created: "+final);
 }finally{if(File.Exists(partial))File.Delete(partial);if(!process.HasExited)process.Kill();await error;}
}else{
 var database=Environment.GetEnvironmentVariable("PGDATABASE")??"";if(!database.StartsWith("ddhi_restore_",StringComparison.Ordinal))throw new InvalidOperationException("Restore test only: target PGDATABASE must start ddhi_restore_ and must be an empty, isolated database.");
 // Authenticate before emitting any restore commands to PostgreSQL.
 using(var check=File.OpenRead(args[2]))BackupCrypt.Decrypt(check,Stream.Null,pass);
 var info=new ProcessStartInfo(args[1]){UseShellExecute=false,RedirectStandardInput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(var a in new[]{"--exit-on-error","--no-owner","--no-privileges","--dbname="+database})info.ArgumentList.Add(a);
 using var process=Process.Start(info)!;var error=process.StandardError.ReadToEndAsync();using(var input=File.OpenRead(args[2]))BackupCrypt.Decrypt(input,process.StandardInput.BaseStream,pass);process.StandardInput.Close();await process.WaitForExitAsync();await error;if(process.ExitCode!=0)throw new InvalidOperationException("Restore failed. Isolated target must be empty; review PostgreSQL logs.");Console.WriteLine("Restore completed to isolated database. Verify record counts, audit chain and clinical workflows before declaring recovery validated.");
}
return 0;

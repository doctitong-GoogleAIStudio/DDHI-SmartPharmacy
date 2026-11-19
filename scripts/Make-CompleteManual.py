"""Create the complete, bookmarked sidebar manual from its editable Markdown source."""
from pathlib import Path
from xml.sax.saxutils import escape
import re
from reportlab.platypus import BaseDocTemplate,PageTemplate,Frame,Paragraph,Spacer,PageBreak,NextPageTemplate,Table,TableStyle,Image
from reportlab.platypus.tableofcontents import TableOfContents
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet,ParagraphStyle
from reportlab.lib import colors

ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'docs/COMPLETE_USER_MANUAL.md'
OUTPUT=ROOT/'docs/DDHI-SmartPharmacy-Complete-User-Manual.pdf'
GREEN=colors.HexColor('#103D31');LIGHT=colors.HexColor('#E0F2E7');GRAY=colors.HexColor('#50665B')
styles=getSampleStyleSheet()
styles.add(ParagraphStyle(name='ManualTitle',fontName='Helvetica-Bold',fontSize=31,leading=36,textColor=GREEN,spaceAfter=20))
styles.add(ParagraphStyle(name='ManualH1',fontName='Helvetica-Bold',fontSize=21,leading=26,textColor=GREEN,spaceAfter=16,keepWithNext=True))
styles.add(ParagraphStyle(name='ManualH2',fontName='Helvetica-Bold',fontSize=12.5,leading=16,textColor=GREEN,spaceBefore=11,spaceAfter=6,keepWithNext=True))
styles.add(ParagraphStyle(name='ManualBody',fontName='Helvetica',fontSize=9.5,leading=14,spaceAfter=7,textColor=GREEN))
styles.add(ParagraphStyle(name='ManualList',parent=styles['ManualBody'],leftIndent=13,firstLineIndent=-10,spaceAfter=6))
styles.add(ParagraphStyle(name='ManualCell',parent=styles['ManualBody'],fontSize=8.7,leading=12,spaceAfter=0))
styles.add(ParagraphStyle(name='ManualToc',fontName='Helvetica',fontSize=9.5,leading=12,leftIndent=0,firstLineIndent=0,spaceBefore=1,textColor=GREEN))

def inline(text):
    text=escape(text)
    text=re.sub(r'\*\*(.+?)\*\*',r'<b>\1</b>',text)
    return re.sub(r'`([^`]+)`',r'<font name="Courier" size="8.5">\1</font>',text)

def furniture(canvas,doc):
    w,h=A4;canvas.saveState();canvas.setFont('Helvetica',8);canvas.setFillColor(GRAY)
    canvas.drawString(46,h-31,'DDHI SMART PHARMACY | COMPLETE USER MANUAL')
    canvas.setStrokeColor(colors.HexColor('#B8D3C4'));canvas.line(46,h-40,w-46,h-40)
    canvas.line(46,39,w-46,39);canvas.drawString(46,26,'Client 0.6.0 | Hospital evaluation | Digos Doctors Hospital, Inc.')
    canvas.drawRightString(w-46,26,str(doc.page));canvas.restoreState()

class Manual(BaseDocTemplate):
    def afterFlowable(self,flowable):
        if getattr(flowable,'manual_chapter',False):
            key=flowable.manual_key;text=flowable.getPlainText()
            self.canv.bookmarkPage(key);self.canv.addOutlineEntry(text,key,level=0,closed=False)
            self.notify('TOCEntry',(0,text,self.page,key))

doc=Manual(str(OUTPUT),pagesize=A4,leftMargin=46,rightMargin=46,topMargin=58,bottomMargin=52,
           title='DDHI Smart Pharmacy - Complete User Manual',author='Digos Doctors Hospital, Inc. / Vicente C. Cavalida Jr. MD')
frame=Frame(46,52,A4[0]-92,A4[1]-110,leftPadding=0,rightPadding=0,topPadding=0,bottomPadding=0)
doc.addPageTemplates([PageTemplate(id='Cover',frames=[frame]),PageTemplate(id='Body',frames=[frame],onPage=furniture)])
story=[Spacer(1,25),Image(str(ROOT/'assets/branding/hospital-logo.png'),width=92,height=92),Spacer(1,27),
       Paragraph('DDHI Smart Pharmacy',styles['ManualTitle']),Paragraph('Complete User Manual',styles['ManualH1']),
       Paragraph('Every sidebar section, generic groups and brands, authorized substitution, patient discharge returns and a glossary.',styles['ManualBody']),Spacer(1,18),
       Paragraph('Digos Doctors Hospital, Inc.',styles['ManualH2']),Paragraph('Developer: Vicente C. Cavalida Jr. MD',styles['ManualBody'])]
cover=Table([[Paragraph('<b>CLIENT 0.6.0</b><br/>Server Control 0.8.0 | API 0.4.0<br/>19 November 2026',styles['ManualBody'])]],colWidths=[A4[0]-92])
cover.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),LIGHT),('BOX',(0,0),(-1,-1),.5,colors.HexColor('#B8D3C4')),('LEFTPADDING',(0,0),(-1,-1),14),('TOPPADDING',(0,0),(-1,-1),12),('BOTTOMPADDING',(0,0),(-1,-1),8)]))
story += [Spacer(1,18),cover,Spacer(1,22),Paragraph('Hospital evaluation edition',styles['ManualH2']),Paragraph('This manual explains software operation. It does not grant clinical, substitution, quality-release or billing authority. Live Cerebro posting and automatic patient refunds remain disabled. Hospital deployment and physical-device acceptance are pending.',styles['ManualBody']),NextPageTemplate('Body'),PageBreak(),Paragraph('Contents',styles['ManualH1'])]
toc=TableOfContents();toc.levelStyles=[styles['ManualToc']];toc.dotsMinLevel=0;story += [toc]

lines=SOURCE.read_text(encoding='utf8').splitlines();start=next(i for i,line in enumerate(lines) if line.startswith('## 1.'))
i=start;chapters=0
while i<len(lines):
    line=lines[i].strip()
    if not line:i+=1;continue
    if line.startswith('## '):
        story.append(PageBreak());chapters+=1;heading=Paragraph(inline(line[3:]),styles['ManualH1']);heading.manual_chapter=True;heading.manual_key=f'chapter-{chapters}';story.append(heading)
    elif line.startswith('### '):story.append(Paragraph(inline(line[4:]),styles['ManualH2']))
    elif line.startswith('|'):
        rows=[]
        while i<len(lines) and lines[i].strip().startswith('|'):
            cells=[c.strip() for c in lines[i].strip().strip('|').split('|')]
            if not all(re.match(r'^:?-+:?$',c) for c in cells):rows.append(cells)
            i+=1
        width=A4[0]-92;count=len(rows[0]);widths=[width*.29,width*.71] if count==2 else [width*.21,width*.32,width*.27,width*.20]
        data=[[Paragraph(inline(c),styles['ManualCell']) for c in row] for row in rows]
        data[0]=[Paragraph('<b>'+inline(c)+'</b>',styles['ManualCell']) for c in rows[0]]
        table=Table(data,colWidths=widths,repeatRows=1,hAlign='LEFT')
        table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),LIGHT),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,colors.HexColor('#F4F8F5')]),('VALIGN',(0,0),(-1,-1),'TOP'),('GRID',(0,0),(-1,-1),.4,colors.HexColor('#CBDCD2')),('LEFTPADDING',(0,0),(-1,-1),8),('RIGHTPADDING',(0,0),(-1,-1),8),('TOPPADDING',(0,0),(-1,-1),8),('BOTTOMPADDING',(0,0),(-1,-1),8)]));story += [table,Spacer(1,10)];continue
    else:story.append(Paragraph(inline(line),styles['ManualList'] if re.match(r'^(- |\d+\. )',line) else styles['ManualBody']))
    i+=1
doc.multiBuild(story)
print(f'{OUTPUT} | {chapters} chapters')

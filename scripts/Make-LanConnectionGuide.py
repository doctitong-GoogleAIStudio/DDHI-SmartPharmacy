"""Build the printable connection guide from its editable Markdown source."""
from pathlib import Path
from xml.sax.saxutils import escape
import re
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak, Table, TableStyle, Preformatted
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import A4

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'docs/SERVER_CLIENT_CONNECTION_GUIDE.md'
OUTPUT = ROOT / 'docs/DDHI-Server-and-Client-Connection-Guide.pdf'
styles = getSampleStyleSheet()
green = colors.HexColor('#146650')
styles.add(ParagraphStyle(name='GuideTitle', fontName='Helvetica-Bold', fontSize=25, leading=29, textColor=green, spaceAfter=12))
styles.add(ParagraphStyle(name='GuideSection', fontName='Helvetica-Bold', fontSize=18, leading=22, textColor=green, spaceAfter=12))
styles.add(ParagraphStyle(name='GuideSub', fontName='Helvetica-Bold', fontSize=11.4, leading=14, textColor=green, spaceBefore=10, spaceAfter=6))
styles.add(ParagraphStyle(name='GuideBody', fontName='Helvetica', fontSize=10, leading=14, spaceAfter=7))
styles.add(ParagraphStyle(name='GuideBullet', parent=styles['GuideBody'], leftIndent=10, firstLineIndent=-7, spaceAfter=5))
styles.add(ParagraphStyle(name='GuideCell', fontName='Helvetica', fontSize=9, leading=11.7, spaceAfter=0))
styles.add(ParagraphStyle(name='GuideCode', fontName='Courier', fontSize=8, leading=11, leftIndent=7, spaceBefore=5, spaceAfter=9, backColor=colors.HexColor('#EEF3F1')))

def inline(text):
    text = escape(text)
    text = re.sub(r'\[([^\]]+)\]\((https?://[^\s)]+)\)', r'<link href="\2" color="#146650"><u>\1</u></link>', text)
    text = re.sub(r'\*\*([^*]+)\*\*', r'<b>\1</b>', text)
    text = re.sub(r'`([^`]+)`', r'<font name="Courier" size="9">\1</font>', text)
    return text

story = []
for page_number, page in enumerate(SOURCE.read_text(encoding='utf8').split('<!-- PAGE -->')):
    if page_number:
        story.append(PageBreak())
    lines = page.strip().splitlines()
    index = 0
    while index < len(lines):
        line = lines[index].strip()
        if not line:
            index += 1
            continue
        if line.startswith('```'):
            code = []
            index += 1
            while index < len(lines) and not lines[index].startswith('```'):
                code.append(lines[index])
                index += 1
            story.append(Preformatted('\n'.join(code), styles['GuideCode']))
        elif line.startswith('|'):
            rows = []
            while index < len(lines) and lines[index].strip().startswith('|'):
                row = [cell.strip() for cell in lines[index].strip().strip('|').split('|')]
                if not all(re.fullmatch(r'[-: ]+', cell) for cell in row):
                    rows.append([Paragraph(inline(cell), styles['GuideCell']) for cell in row])
                index += 1
            width = A4[0] - 92
            first = 170 if page_number == 7 else 151
            table = Table(rows, colWidths=[first, width-first], repeatRows=1, hAlign='LEFT')
            table.setStyle(TableStyle([
                ('BACKGROUND', (0,0), (-1,0), colors.HexColor('#DFECE6')),
                ('VALIGN', (0,0), (-1,-1), 'TOP'),
                ('LINEBELOW', (0,0), (-1,0), .6, green),
                ('LINEBELOW', (0,1), (-1,-1), .25, colors.HexColor('#CCDAD3')),
                ('LEFTPADDING', (0,0), (-1,-1), 7), ('RIGHTPADDING', (0,0), (-1,-1), 7),
                ('TOPPADDING', (0,0), (-1,-1), 6), ('BOTTOMPADDING', (0,0), (-1,-1), 6),
            ]))
            story.extend([table, Spacer(1,8)])
            continue
        elif line.startswith('# '):
            story.append(Paragraph(inline(line[2:]), styles['GuideTitle']))
        elif line.startswith('## '):
            style = styles['GuideSub'] if page_number == 0 else styles['GuideSection']
            story.append(Paragraph(inline(line[3:]), style))
        elif line.startswith('### '):
            story.append(Paragraph(inline(line[4:]), styles['GuideSub']))
        else:
            style = styles['GuideBullet'] if re.match(r'^(\d+\. |[-] )', line) else styles['GuideBody']
            line = line.replace('- [ ] ', '[ ] ', 1) if line.startswith('- [ ] ') else line
            story.append(Paragraph(inline(line), style))
        index += 1

def furniture(canvas, doc):
    w,h = A4
    canvas.saveState()
    canvas.setStrokeColor(green)
    canvas.setLineWidth(.7)
    canvas.line(46,h-39,w-46,h-39)
    canvas.setFont('Helvetica',8)
    canvas.setFillColor(colors.HexColor('#546E62'))
    canvas.drawString(46,h-30,'DDHI SMART PHARMACY | SERVER + CLIENT CONNECTIONS')
    canvas.line(46,37,w-46,37)
    canvas.drawString(46,24,'Evaluation build | 15 November 2026 | Digos Doctors Hospital, Inc.')
    canvas.drawRightString(w-46,24,str(doc.page))
    canvas.restoreState()

SimpleDocTemplate(str(OUTPUT), pagesize=A4, rightMargin=46, leftMargin=46,
                  topMargin=56, bottomMargin=49,
                  title='DDHI Smart Pharmacy - Server and Client Connection Guide',
                  author='Digos Doctors Hospital, Inc.').build(story, onFirstPage=furniture, onLaterPages=furniture)
print(OUTPUT)

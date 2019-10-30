import urllib
import datetime
txt = urllib.URLopener()
tdate = datetime.datetime.strftime(datetime.date.today(), "%Y%m%d")
txt.retrieve('http://www.ffwc.gov.bd/images/fdry.pdf', r'C:\Users\nbiswas\Desktop\Nishan\SASWE\FFWC_Flood\NewDesign\Database\DryPeriodData\FFWC_' + tdate + '.pdf')

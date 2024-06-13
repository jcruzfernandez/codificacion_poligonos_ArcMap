# -*- coding: utf-8 -*-
import arcpy
import sys
reload(sys)
sys.setdefaultencoding('utf-8')
import math

feature_name = arcpy.GetParameterAsText(0)
feature_layer = arcpy.GetParameterAsText(1)

arcpy.SpatialJoin_analysis(feature_name,feature_layer,"in_memory","JOIN_ONE_TO_ONE","KEEP_ALL","","INTERSECT","","")
# -*- coding: utf-8 -*-
import arcpy
import sys
reload(sys)
sys.setdefaultencoding('utf-8')
import math

feature_name = arcpy.GetParameterAsText(0)
feature_layer = arcpy.GetParameterAsText(1)
field_order = arcpy.GetParameterAsText(2)

# Fields
_OID = "OID@"
_GEOM = "SHAPE@"
_TARGET = "TARGET_FID"
_CODE = "CODIGO"

lyr_spj = arcpy.SpatialJoin_analysis(feature_name,feature_layer,"in_memory\lyr_spj","JOIN_ONE_TO_ONE","KEEP_ALL","","INTERSECT","","")

mxd = arcpy.mapping.MapDocument("current")
df = arcpy.mapping.ListDataFrames(mxd)[0]

# Crear una nueva capa
new_layer = arcpy.mapping.Layer("in_memory\lyr_spj")

# Añadir la capa al dataframe
# arcpy.mapping.AddLayer(df, new_layer, "TOP")

dicci = dict([key,(val_1,)] for key, val_1 in arcpy.da.SearchCursor(lyr_spj,[_TARGET, field_order]))
#VAMOS A DEFINIR UPDATE
with arcpy.da.UpdateCursor(feature_name,[_CODE, _OID]) as cursorU:
    for val_1, key in cursorU:
        arcpy.AddMessage(str(key))
        if not key in dicci:
            arcpy.AddMessage(str(key)+"<<")
            continue
        else:
            indice = dicci[key][0]
            if indice <= 10:  
                codigo = '0{}'.format(str(indice))
            else:
                codigo = '{}'.format(str(indice))
            row = (codigo, key)
            arcpy.AddMessage(str(row))
            cursorU.updateRow(row)
del cursorU
# Refrescar el mapa y la tabla de contenido
arcpy.RefreshActiveView()
arcpy.RefreshTOC()
arcpy.AddMessage(str(dicci))

# -*- coding: utf-8 -*-
import arcpy
import sys
reload(sys)
sys.setdefaultencoding('utf-8')
import math

# Configura el entorno
#arcpy.env.workspace = 'C:/Tu/Directorio/de/Trabajo'
_OID ='OID@'
_XY = 'SHAPE@XY'
_CODFIELD='CODIGO'
_FIELD_PRED ='PREDIO'
# _AREA_AFECT = 'AREA_AFECT'

# Ajusta al nombre de tu capa
layer_name = arcpy.GetParameterAsText(0)
field_name = arcpy.GetParameterAsText(1)
# _CODFIELD = field_name

# predio, area_afect, ini_xy= [(x[0], x[1], x[2]) for x in arcpy.da.SearchCursor(layer_name, [_FIELD_PRED, _AREA_AFECT, _XY])][0]
predio, ini_xy= [(x[0], x[1]) for x in arcpy.da.SearchCursor(layer_name, [field_name, _XY])][0]
# arcpy.SelectLayerByAttribute_management(layer_name, "NEW_SELECTION", "{} ='{}' AND {}='{}'".format(_FIELD_PRED, predio, _AREA_AFECT, area_afect))
arcpy.SelectLayerByAttribute_management(layer_name, "NEW_SELECTION", "{} ='{}'".format(field_name, predio))

# Encuentra el punto inicial (podría ser el más al norte, el más al este, etc.)
# Este paso depende de cómo determines tu punto inicial
punto_inicial = ini_xy#(645700.0543, 8564227.722)
# Calcula el ángulo de cada punto respecto al punto inicial
puntos_y_angulos = []
with arcpy.da.SearchCursor(layer_name, [_OID,_XY]) as cursor:
    for oid, xy in cursor:
        dx = xy[0] - punto_inicial[0]
        dy = xy[1] - punto_inicial[1]
        angulo = math.atan2(dy, dx)
        if angulo == 0.0:
            puntos_y_angulos.append((oid, 99))
        else:
            puntos_y_angulos.append((oid, angulo))

# Ordena los puntos por su ángulo en orden horario
puntos_ordenados = sorted(puntos_y_angulos, key=lambda x: x[1], reverse=True)

# Asigna códigos y actualiza la capa de puntos
contador = 1

def alfanumerico():
    global contador
    with arcpy.da.UpdateCursor(layer_name, [_OID, _CODFIELD]) as cursor:
        for oid, _ in cursor:
            # Encuentra el índice del punto en la lista ordenada
            indice = next(i for i, v in enumerate(puntos_ordenados) if v[0] == oid)
            # Genera el código basado en el índice
            codigo = '01{}'.format(chr(65 + indice))  # Esto generará códigos como 01A, 01B, etc.
            if indice >= 26:  # Ajusta según necesites para más de 26 puntos
                contador += 1
                codigo = '0{}{}'.format(contador,chr(65 + indice - 26))
            
            # Actualiza el campo con el nuevo código
            cursor.updateRow([oid, codigo])

def numerico():
    # global contador
    with arcpy.da.UpdateCursor(layer_name, [_OID, _CODFIELD]) as cursor:
        for oid, _ in cursor:
            # Encuentra el índice del punto en la lista ordenada
            indice = next(i for i, v in enumerate(puntos_ordenados) if v[0] == oid)
            # Genera el código basado en el índice
            if indice + 1 <= 10:  
                codigo = '0{}'.format(str(indice+1))
            else:
                codigo = '{}'.format(str(indice+1))  # Esto generará códigos como 01, 02, etc.
            # Actualiza el campo con el nuevo código
            # contador += 1
            cursor.updateRow([oid, codigo])

numerico()

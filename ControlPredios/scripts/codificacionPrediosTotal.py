# -*- coding: utf-8 -*-
import arcpy
import sys
reload(sys)
sys.setdefaultencoding('utf-8')
import math
import traceback

# Configura el entorno
#arcpy.env.workspace = 'C:/Tu/Directorio/de/Trabajo'
_OID ='OID@'
_XY = 'SHAPE@XY'
_SHAPE = 'SHAPE@'
_CODFIELD='CODIGO'
_FIELD_PRED ='PREDIO'
# _AREA_AFECT = 'AREA_AFECT'

# Ajusta al nombre de tu capa
layer_name = arcpy.GetParameterAsText(0)
field_name = arcpy.GetParameterAsText(1)
# _CODFIELD = field_name

def indexacion(tuplas):
    # Diccionario para rastrear las coordenadas y asignar índices
    coord_dict = {}
    index = 1
    # Lista para almacenar el resultado final
    resultado = []

    for tupla in tuplas:
        coord = tupla[1]
        if coord in coord_dict:
            idx = coord_dict[coord]
        else:
            idx = index
            coord_dict[coord] = idx
            index += 1
        resultado.append((idx, tupla))
    return resultado

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

def numerico_v2(field_name, cod_agrupado):
    codigos_asignados = {}
    contador = 1
    with arcpy.da.UpdateCursor(layer_name, [_OID, _CODFIELD], "{} = '{}'".format(field_name, cod_agrupado)) as cursor:
        for oid, _ in cursor:
            for i, (oid_ord, xy, angulo) in indexacion(puntos_ordenados):
                if oid == oid_ord:
                    # Genera el código basado en el índice
                    centroide = xy #puntos_ordenados[i][1]
                    if centroide in codigos_asignados:
                        codigo = codigos_asignados[centroide]
                    else:
                        codigo = '0{}'.format(str(i)) if i <= 9 else '{}'.format(str(i))
                        codigos_asignados[centroide] = codigo
                        contador += 1
                    # Actualiza el campo con el nuevo código
                    cursor.updateRow([oid, codigo])
                    break

# predio, ini_xy= [(x[0], x[1]) for x in arcpy.da.SearchCursor(layer_name, [field_name, _XY])][0]
# arcpy.SelectLayerByAttribute_management(layer_name, "NEW_SELECTION", "{} ='{}' AND {}='{}'".format(_FIELD_PRED, predio, _AREA_AFECT, area_afect))
arcpy.SelectLayerByAttribute_management(layer_name, "CLEAR_SELECTION")

# Lista para almacenar los códigos de manzana
codigos_agrupados = []

try:
    # Obtener una lista de todos los códigos de manzana únicos
    with arcpy.da.SearchCursor(layer_name, [field_name]) as cursor:
        for row in cursor:
            if row[0] not in codigos_agrupados:
                codigos_agrupados.append(row[0])

    # Recorrer cada grupo de predios por manzana
    for cod_agrupado in codigos_agrupados:
        # Lista para almacenar los centroides y OID de los predios
        centroides = []
        # Obtener los centroides de los predios para la manzana actual
        with arcpy.da.SearchCursor(layer_name, [_OID, _SHAPE], "{} = '{}'".format(field_name, cod_agrupado)) as cursor:
            for row in cursor:
                oid = row[0]
                shape = row[1]
                centroide = shape.centroid
                centroides.append((oid, centroide))

        # Encontrar el predio con el centroide más al norte y este
        if centroides:
            max_norte_este = max(centroides, key=lambda c: (c[1].Y, c[1].X))
            oid_max = max_norte_este[0]

            # Encuentra el punto inicial (podría ser el más al norte, el más al este, etc.)
            # Este paso depende de cómo determines tu punto inicial
            punto_inicial = (max_norte_este[1].X, max_norte_este[1].Y) #ini_xy
            # Calcula el ángulo de cada punto respecto al punto inicial
            puntos_y_angulos = []
            with arcpy.da.SearchCursor(layer_name, [_OID, _XY], "{} = '{}'".format(field_name, cod_agrupado)) as cursor:
                for oid, xy in cursor:
                    dx = xy[0] - punto_inicial[0]
                    dy = xy[1] - punto_inicial[1]
                    angulo = math.atan2(dy, dx)
                    if angulo == 0.0:
                        puntos_y_angulos.append((oid, xy, 99))
                    else:
                        puntos_y_angulos.append((oid, xy, angulo))

            # Ordena los puntos por su ángulo en orden horario
            puntos_ordenados = sorted(puntos_y_angulos, key=lambda x: x[2], reverse=True)
            # Asigna códigos y actualiza la capa de puntos
            numerico_v2(field_name, cod_agrupado)
except Exception as e:
    arcpy.AddError("Error al actualizar los códigos: {}".format(traceback.format_exc()))
#numerico()

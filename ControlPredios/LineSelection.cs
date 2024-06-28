using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Reflection;

namespace ControlPredios
{
    public class LineSelection : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        INewLineFeedback newLineFeedBack;
        IActiveView vista;
        IMap mapa;
        bool mousePresionado;
        int indice = 1;
        string layerName;
        string fieldOrderName = "ORDEN";
        string layerNameTempPoints = "PuntosTemporales";
        List<KeyValuePair<int, IPoint>> puntosConIndice = new List<KeyValuePair<int, IPoint>>();
        private static List<IElement> elementosPunto = new List<IElement>();
        private ISpatialReference spatialReference;
        private static string currentPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string pathTool = System.IO.Path.Combine(currentPath, "scripts", "ToolboxMain.tbx");

        public LineSelection()
        {
        }
        protected override void OnActivate()
        {
            // Cambiar el cursor a una cruz cuando la herramienta se activa
            //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
            Cursor = System.Windows.Forms.Cursors.Cross;
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {
            //definimos vista y mapa
            vista = ArcMap.Document.ActivatedView;
            mapa = ArcMap.Document.FocusMap;
            IPoint punto = vista.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);

            if (newLineFeedBack == null)
            {
                newLineFeedBack = new NewLineFeedback();
                newLineFeedBack.Display = vista.ScreenDisplay;
                newLineFeedBack.Start(punto);
                //newLineFeedBack.AddPoint(punto);
                //puntosConIndice.Add(new KeyValuePair<int, IPoint>(indice,punto));
                //DrawPointClick(punto, mapa);
            }
            else
            {
                newLineFeedBack.AddPoint(punto);
                //DrawPointClick(punto, mapa);
            }
            puntosConIndice.Add(new KeyValuePair<int, IPoint>(indice, punto));
            mousePresionado = true;
            DrawPointClick(punto, mapa, vista);
            indice++;
            //vista.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            //vista.Refresh();
        }

        protected override void OnMouseMove(MouseEventArgs arg)
        {
            if (mousePresionado)
            {
                IPoint punto = vista.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
                newLineFeedBack.MoveTo(punto);
            }
            else
            {
                return;
            }
        }

        protected override void OnDoubleClick()
        {
            vista = ArcMap.Document.ActivatedView;
            mapa = ArcMap.Document.FocusMap;
            IPolyline linea = newLineFeedBack.Stop();
            if (linea != null)
            {
                mapa.SelectByShape(linea, null, false);
                vista.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            mousePresionado = false;
            newLineFeedBack = null;
            EliminarPuntos(mapa, vista);
            EliminarCapaPuntosTemp();
            IFeatureLayer featureLayerPoints = CrearFeatureLayerEnMemoria(puntosConIndice);
            //layerName = ComboBox1.selectedLayerName;
            for (int i = 0; i < mapa.LayerCount; i++)
            {
                ILayer layer = mapa.get_Layer(i);
                if (layer is IFeatureLayer) // Asegúrate de que la capa sea una capa de entidades
                {
                    IFeatureLayer featureLayer = (IFeatureLayer)layer;
                    IFeatureClass featureClass = featureLayer.FeatureClass;
                    if (featureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        // Crea un buffer alrededor del punto de clic si es necesario para mejorar la selección
                        // o ajusta la geometría de selección según tus necesidades
                        IGeometry selectGeom = puntosConIndice[0].Value as IGeometry;
                        // Verifica si el punto de clic intersecta alguna entidad de esta capa
                        if (PointSelection.IsFeatureIntersected(selectGeom, featureLayer) & layer.Name == ComboBox1.selectedLayerName)
                        {
                            layerName = layer.Name;
                            //MessageBox.Show("Capa seleccionada: " + layerName);

                            // Aquí puedes realizar el geoprocesamiento necesario con la capa seleccionada
                            // Por ejemplo, pasando el layerName o el featureLayer a otro método
                            break; // Sale del bucle si ya encontraste una capa que contiene el punto
                        }
                    }
                }
            }

            IGeoProcessor2 geoprocessor = new GeoProcessorClass();
            // Opcional: Configura el geoprocesador para sobrescribir la salida
            geoprocessor.OverwriteOutput = true;
            // Agregar el geoproceso al historial de geoprocesamiento de ArcMap
            geoprocessor.AddToResults = true;
            try
            {
                //MessageBox.Show(pathTool);
                geoprocessor.AddToolbox(pathTool);
                // Crea un objeto IVariantArray para almacenar los parámetros de la herramienta
                IVariantArray parameters = new VarArrayClass();
                parameters.Add(layerName);
                parameters.Add(layerNameTempPoints);
                parameters.Add(fieldOrderName);
                IGeoProcessorResult results = geoprocessor.Execute("codificacionPrediosLine", parameters, null);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al ejecutar la herramienta: " + e.Message);
                // Manejo adicional de errores aquí
            }
            // Limpiar la lista antes de reutilizarla
            puntosConIndice.Clear();
            indice = 1;
        }

        protected override void OnUpdate()
        {
            Enabled = ComboBox_field.selectedFieldName != null;
        }

        public static IElement DrawPointClick(IPoint point, IMap map, IActiveView vista)
        {
            // Crear un color para el símbolo
            IRgbColor color = new RgbColorClass();
            color.Red = 167;
            color.Green = 45;
            color.Blue = 255;

            // Crear un símbolo para el punto
            ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
            simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCross;  //esriSimpleMarkerStyle.esriSMSCircle;
            simpleMarkerSymbol.Size = 11;
            simpleMarkerSymbol.Angle = 45;
            simpleMarkerSymbol.Color = color;

            // Crear un elemento de punto y asignarle el símbolo
            IMarkerElement markerElement = new MarkerElementClass();
            markerElement.Symbol = simpleMarkerSymbol;
            IElement element = (IElement)markerElement;
            element.Geometry = point;

            // Agregar el elemento de punto al mapa
            IGraphicsContainer graphicsContainer = (IGraphicsContainer)map;
            graphicsContainer.AddElement(element, 0);
            elementosPunto.Add(element);

            // Crear un envelope pequeño alrededor del punto
            IEnvelope envelope = new EnvelopeClass();
            envelope.PutCoords(point.X - 2, point.Y - 2, point.X + 2, point.Y + 2); // El tamaño del envelope depende de la escala del mapa
            envelope.Expand(3, 3, false); // Expandir el envelope por 3 unidades en todas las direcciones

            // Refrescar solo la región del envelope
            vista.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelope);
            return element;
        }

        public static void EliminarPuntos(IMap map, IActiveView vista)
        {
            // Asegúrate de que estás en el contexto correcto, como se mostró previamente
            IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
            // Verificar si el contenedor gráfico no es nulo
            if (graphicsContainer != null)
            {
                // Eliminar todos los elementos gráficos
                graphicsContainer.DeleteAllElements();
                // Refrescar la vista para reflejar los cambios
                IActiveView activeView = ArcMap.Document.ActiveView;
                activeView.Refresh();
            }
        }

        public void EliminarCapaPuntosTemp()
        {
            // Obtener el documento de ArcMap y el mapa enfocado
            IMxDocument mxDoc = (IMxDocument)ArcMap.Application.Document;
            IMap map = mxDoc.FocusMap;
            // IFeatureClass featureClass;
            // Agregar todas las capas del mapa al ComboBox
            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer layer = map.get_Layer(i);
                if (layer is IFeatureLayer)
                {
                    IFeatureLayer featureLayer = (IFeatureLayer)layer;
                    if (layer.Name.Equals(layerNameTempPoints, StringComparison.OrdinalIgnoreCase))
                    {
                        //featureLayer = layer as IFeatureLayer;
                        //featureClass = featureLayer.FeatureClass;
                        //// Eliminar todos los registros de la capa
                        //ITable table = featureClass as ITable;
                        //ICursor cursor = table.Update(null, false);
                        //IRow row;
                        //while ((row = cursor.NextRow()) != null)
                        //{
                        //    row.Delete();
                        //    //row = cursor.NextRow();
                        //}
                        //cursor.Flush();
                        // Eliminar la capa del mapa
                        map.DeleteLayer(layer);
                        break; // Salir del bucle después de eliminar la capa
                    }
                }
            }
        }

        public IFeatureLayer CrearFeatureLayerEnMemoria(List<KeyValuePair<int, IPoint>> listaPuntos)
        {
            // Obtener el documento actual de ArcMap
            IMxDocument mxDocument = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDocument.FocusMap;
            IFeatureLayer featureLayer = null;
            IFeatureClass featureClass;

            //// Verificar si existe un FeatureLayer con el nombre especificado y eliminalo
            //for (int i = map.LayerCount - 1; i >= 0; i--)
            //{
            //    ILayer layer = map.get_Layer(i);
            //    if (layer is IFeatureLayer && layer.Name.Equals(layerNameTempPoints, StringComparison.OrdinalIgnoreCase))
            //    {
            //        featureLayer = layer as IFeatureLayer;
            //        featureClass = featureLayer.FeatureClass;

            //        // Eliminar todos los registros de la capa
            //        ITable table = featureClass as ITable;
            //        ICursor cursor = table.Update(null, false);
            //        IRow row;
            //        while ((row = cursor.NextRow()) != null)
            //        {
            //            cursor.DeleteRow();
            //        }
            //        cursor.Flush();
            //        map.DeleteLayer(layer);
            //    }
            //}

            // Verificar si hay al menos una capa en el mapa
            if (map.LayerCount > 0)
            {
                // Obtener la primera capa del mapa
                ILayer layer = map.get_Layer(0);

                // Verificar si la capa tiene un dataset válido y obtener su spatial reference
                if (layer is IFeatureLayer)
                {
                    featureLayer = layer as IFeatureLayer;
                    featureClass = featureLayer.FeatureClass;
                    if (featureClass != null)
                    {
                        // Obtener el spatial reference del dataset de la capa
                        IDataset dataset = featureClass as IDataset;
                        IGeoDataset geoDataset = dataset as IGeoDataset;
                        spatialReference = geoDataset.SpatialReference;
                    }
                }
            }
            // Crear un workspace en memoria
            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName workspaceName = workspaceFactory.Create("", "GPInMemoryWorkspace", null, 0);
            IName name = (IName)workspaceName;
            IWorkspace inMemoryWorkspace = (IWorkspace)name.Open();

            // Crear un FeatureClass en memoria
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            // Crear un campo para el OID
            IField idField = new FieldClass();
            IFieldEdit idFieldEdit = (IFieldEdit)idField;
            idFieldEdit.Name_2 = "OBJECTID";
            idFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            idFieldEdit.IsNullable_2 = false;
            idFieldEdit.Required_2 = false;
            fieldsEdit.AddField(idField);
            
            //Crear campo ORDEN
            IField ordField = new FieldClass();
            IFieldEdit ordFieldEdit = (IFieldEdit)ordField;
            ordFieldEdit.Name_2 = fieldOrderName;
            ordFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(ordField);

            // Crear un campo de geometría para los puntos
            IField geometryField = new FieldClass();
            IFieldEdit geometryFieldEdit = (IFieldEdit)geometryField;
            //geometryFieldEdit.Name_2 = "SHAPE";
            //geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            geometryDefEdit.SpatialReference_2 = spatialReference;
            geometryFieldEdit.Name_2 = "SHAPE";
            geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            geometryFieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(geometryField);
            
            // Crear el FeatureClass
            IFeatureClass featureClassmain = ((IFeatureWorkspace)inMemoryWorkspace).CreateFeatureClass("MyFeatureClass", fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            // Añadir los puntos de la lista al FeatureClass
            foreach (var par in listaPuntos)
            {
                IFeature feature = featureClassmain.CreateFeature();
                feature.set_Value(feature.Fields.FindField(fieldOrderName), par.Key);
                feature.Shape = par.Value;
                feature.Store();
            }

            // Crear un FeatureLayer a partir del FeatureClass
            IFeatureLayer featureLayermain = new FeatureLayerClass();
            featureLayermain.FeatureClass = featureClassmain;
            featureLayermain.Name = layerNameTempPoints;

            // Agregar el feature layer creado al mapa
            if (map != null && featureLayermain != null)
            {
                map.AddLayer(featureLayermain);

                // Opcional: Si quieres que el layer se agregue en una posición específica
                // map.AddLayerAt(índice, featureLayer);
            }

            // Refrescar la vista de ArcMap para mostrar el nuevo layer
            mxDocument.UpdateContents();
            mxDocument.ActiveView.Refresh();

            return featureLayer;
        }
    }
}
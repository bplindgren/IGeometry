using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Desktop.AddIns;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Phonatech
{
    public class AddTower : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AddTower()
        {

        }

        protected override void OnUpdate()
        {

        }

        protected override void OnMouseUp(MouseEventArgs arg)
        {
            try
            {
                int x = arg.X;
                int y = arg.Y;

                IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;

                IFeatureLayer pfeaturelayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
                IDataset pDS = (IDataset)pfeaturelayer.FeatureClass;
                TowerManager tm = new TowerManager(pDS.Workspace);

                // THIS IS HOW YOU CREATE A POINT
                IPoint pPoint = pMxdoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

                Tower t = tm.GetNearestTower(pPoint, 30);

                if (t == null)
                {
                    MessageBox.Show("No towers were found within the are you clicked");
                    return;
                }

                MessageBox.Show("Tower id " + t.ID + Environment.NewLine + "Type: " + t.TowerType + Environment.NewLine + "NetworkBand " + t.NetworkBand);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

}

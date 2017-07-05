using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace Phonatech
{
    public class GenerateTowersRanges : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenerateTowersRanges()
        {
        }

        protected override void OnClick()
        {
            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset)pfeaturelayer.FeatureClass;
            TowerManager tm = new TowerManager(pDS.Workspace);

            
            Tower pTower = tm.GetTowerByID("T04");

            // range of 100 meters
            int towerRange = 100;
            ITopologicalOperator pTopo = (ITopologicalOperator)pTower.TowerLocation;
            IPolygon range3Bars = (IPolygon)pTopo.Buffer(towerRange / 3);

            IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer(towerRange * 2 / 3);
            ITopologicalOperator pIntTopo = (ITopologicalOperator)range2BarsWhole;
            IPolygon range2BarsDonut = (IPolygon)pIntTopo.SymmetricDifference(range3Bars);

            IPolygon range1BarWhole = (IPolygon)pTopo.Buffer(towerRange);
            ITopologicalOperator pIntTopo2 = (ITopologicalOperator)range1BarWhole;
            IPolygon range1BarDonut = (IPolygon)pIntTopo2.SymmetricDifference(range2BarsWhole);

            // Start editing session to create features
            IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDS.Workspace;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            // Get feature class to edit
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;
            IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("TowerRange");

            // Create range 3 bars
            IFeature pFeature3Bar = pTowerRangeFC.CreateFeature();
            pFeature3Bar.set_Value(pFeature3Bar.Fields.FindField("TOWERID"), "T04");
            pFeature3Bar.set_Value(pFeature3Bar.Fields.FindField("RANGE"), 3);
            pFeature3Bar.Shape = range3Bars;
            pFeature3Bar.Store();

            // Create range 2 bars
            IFeature pFeature2Bar = pTowerRangeFC.CreateFeature();
            pFeature2Bar.set_Value(pFeature2Bar.Fields.FindField("TOWERID"), "T04");
            pFeature2Bar.set_Value(pFeature2Bar.Fields.FindField("RANGE"), 2);
            pFeature2Bar.Shape = range2BarsDonut;
            pFeature2Bar.Store();

            // Create range 1 bar
            IFeature pFeature1Bar = pTowerRangeFC.CreateFeature();
            pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("TOWERID"), "T04");
            pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("RANGE"), 1);
            pFeature1Bar.Shape = range1BarDonut;
            pFeature1Bar.Store();

            // End editing session
            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}

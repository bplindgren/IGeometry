using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class TowerManager
    {

        private IWorkspace _workspace;

        private DataTable _towerdetails;

        public TowerManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;

            //read the tower details table
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
            ITable pTableTowerDetails = pFWorkspace.OpenTable("TowerDetails");
            ICursor pCursor = pTableTowerDetails.Search(null, false);
            IRow pRow = pCursor.NextRow();
            _towerdetails = new DataTable();


            _towerdetails.Columns.Add("TowerType");
            _towerdetails.Columns.Add("TowerCoverage");
            _towerdetails.Columns.Add("TowerCost");
            _towerdetails.Columns.Add("TowerHeight");
            _towerdetails.Columns.Add("TowerBaseArea");


            while (pRow != null)
            {

                DataRow dtRow = _towerdetails.NewRow();
                dtRow["TowerType"] = pRow.get_Value(pRow.Fields.FindField("TowerType"));
                dtRow["TowerCoverage"] = pRow.get_Value(pRow.Fields.FindField("TowerCoverage"));
                dtRow["TowerCost"] = pRow.get_Value(pRow.Fields.FindField("TowerCost"));
                dtRow["TowerHeight"] = pRow.get_Value(pRow.Fields.FindField("TowerHeight"));
                dtRow["TowerBaseArea"] = pRow.get_Value(pRow.Fields.FindField("TowerBaseArea"));

                _towerdetails.Rows.Add(dtRow);
                _towerdetails.AcceptChanges();

                pRow = pCursor.NextRow();
            }
        }





        /// <summary>
        /// Generate the tower coverage
        /// </summary>
        public void GenerateTowerCoverage(Towers pTowers)
        {
            IWorkspaceEdit pWorkspaceEdit;
            pWorkspaceEdit = (IWorkspaceEdit)this._workspace;
            try
            {
                IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;

                IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("TowerRange");

                pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();

                //delete all ranges , we should later change that to delete only dirty towers
                IFeatureCursor pcursor = pTowerRangeFC.Update(null, false);
                IFeature pfeaturerange = pcursor.NextFeature();
                while (pfeaturerange != null)
                {
                    //we need to change that later 
                    pfeaturerange.Delete();

                    pfeaturerange = pcursor.NextFeature();
                }

                foreach (Tower pTower in pTowers.Items)
                {

                    ITopologicalOperator pTopo = (ITopologicalOperator)pTower.TowerLocation;

                    IPolygon range3Bars = (IPolygon)pTopo.Buffer(pTower.TowerCoverage / 3);
                    IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer((pTower.TowerCoverage * 2) / 3);
                    IPolygon range1BarsWhole = (IPolygon)pTopo.Buffer(pTower.TowerCoverage);

                    ITopologicalOperator pIntTopo = (ITopologicalOperator)range2BarsWhole;

                    ITopologicalOperator pIntTopo1 = (ITopologicalOperator)range1BarsWhole;


                    IPolygon range2BarsDonut = (IPolygon)pIntTopo.SymmetricDifference(range3Bars); //,esriGeometryDimension.esriGeometry2Dimension); 
                    IPolygon range1BarsDonut = (IPolygon)pIntTopo1.SymmetricDifference(range2BarsWhole); //,esriGeometryDimension.esriGeometry2Dimension); 


                    IFeature pFeature = pTowerRangeFC.CreateFeature();

                    pFeature.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
                    pFeature.set_Value(pFeature.Fields.FindField("RANGE"), 3);

                    pFeature.Shape = range3Bars;
                    pFeature.Store();


                    IFeature pFeature2Bar = pTowerRangeFC.CreateFeature();

                    pFeature2Bar.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
                    pFeature2Bar.set_Value(pFeature.Fields.FindField("RANGE"), 2);

                    pFeature2Bar.Shape = range2BarsDonut;
                    pFeature2Bar.Store();


                    IFeature pFeature1Bar = pTowerRangeFC.CreateFeature();

                    pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("TOWERID"), "T04");
                    pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("RANGE"), 1);

                    pFeature1Bar.Shape = range1BarsDonut;
                    pFeature1Bar.Store();
                }
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);

            }
            catch (Exception ex)
            {
                //if anything went wrong, just roll back
                pWorkspaceEdit.AbortEditOperation();
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

        }
        

        public Tower GetTower(IFeature pTowerFeature)
        {
            Tower tower = new Tower();
            tower.ID = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERID")); ;
            tower.NetworkBand = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("NETWORKBAND"));
            tower.TowerType = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERTYPE"));
            tower.TowerLocation = (IPoint)pTowerFeature.Shape;

            //search for the tower details ..

            foreach (DataRow r in _towerdetails.Rows)
                if (r["TowerType"].ToString() == tower.TowerType)
                {
                    tower.TowerCoverage = double.Parse(r["TowerCoverage"].ToString());
                    tower.TowerCost = double.Parse(r["TowerCost"].ToString());
                    tower.TowerBaseArea = double.Parse(r["TowerBaseArea"].ToString());
                    tower.TowerHeight = double.Parse(r["TowerHeight"].ToString());
                }

            return tower;
        }

        public Tower GetTowerByID(string towerid)
        {
            //query the geodatabase.. 
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;

            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

            //get the tower feature by id 
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "TOWERID = '" + towerid + "'";

            IFeatureCursor pFCursor = fcTower.Search(pQFilter, true);

            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
                return null;
            
            return GetTower(pTowerFeature);
            //get the tower type, and then query the tower details table to get the rest of the data...
        }

        public Tower GetNearestTower(IPoint pPoint, int buffer)
        {
            ITopologicalOperator pTopo = (ITopologicalOperator)pPoint;

            IGeometry pBufferedPoint = pTopo.Buffer(buffer);

            ISpatialFilter pSFilter = new SpatialFilter();
            pSFilter.Geometry = pBufferedPoint;
            pSFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            //query the geodatabase.. 
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;

            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

            IFeatureCursor pFCursor = fcTower.Search(pSFilter, true);
            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
                return null;

            return GetTower(pTowerFeature);
        }

        /// <summary>
        /// return all towers
        /// </summary>
        /// <returns></returns>
        public Towers GetTowers()
        {
            Towers towers = new Towers();
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass pTowerFC = pFWorkspace.OpenFeatureClass("Towers");

            IFeatureCursor pFcursor = pTowerFC.Search(null, false);
            IFeature pFeature = pFcursor.NextFeature();
            while (pFeature != null)
            {
                Tower tower = this.GetTower(pFeature);
                towers.Items.Add(tower);
                pFeature = pFcursor.NextFeature();
            }
            return towers;
        }
    }
}

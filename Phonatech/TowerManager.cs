using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonatech
{
    public class TowerManager
    {

        private IWorkspace _workspace;
        private DataTable _towerdetails;

        public TowerManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;

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

        public Tower GetTower(IFeature pTowerFeature)
        {
            Tower tower = new Tower();
            tower.ID = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERID"));
            tower.NetworkBand = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("NETWORKBAND"));
            tower.TowerType = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERTYPE"));
            tower.TowerLocation = (IPoint) pTowerFeature.Shape;
            
            // Search for the tower details record
            foreach(DataRow r in _towerdetails.Rows)
            {
                if (r["TowerType"].ToString() == tower.TowerType)
                {
                    tower.TowerCoverage = double.Parse(r["TowerCoverage"].ToString());
                    tower.TowerCost = double.Parse(r["TowerCost"].ToString());
                    tower.TowerBaseArea = double.Parse(r["TowerBaseArea"].ToString());
                    tower.TowerHeight = double.Parse(r["TowerHeight"].ToString());
                }
            }

            return tower;
        }

        public Tower GetTowerByID(string towerid)
        {
            // query the geodatabase 
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace) _workspace;
            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

            // get the tower feature by id
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "TOWERID = '" + towerid + "'";

            IFeatureCursor pFCursor = fcTower.Search(pQFilter, true);
            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
                return null;

            return GetTower(pTowerFeature);
        }

        public Tower GetNearestTower(IPoint pPoint, int buffer)
        {
            ITopologicalOperator pTopo = (ITopologicalOperator) pPoint;

            IGeometry pBufferedPoint = pTopo.Buffer(buffer);

            ISpatialFilter pSFilter = new SpatialFilter();
            pSFilter.Geometry = pBufferedPoint;
            pSFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            // This is how you access the "Towers" feature class
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

            IFeatureCursor pFCursor = fcTower.Search(pSFilter, true);
            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
                return null;

            return GetTower(pTowerFeature);
        }

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

            }
            catch
            {
                Console.WriteLine("HI");
            }
        }
    }
}


﻿using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Rhino;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Scan2BIM
{
    public class GH_PointCloudParam : GH_PersistentGeometryParam<GH_PointCloud>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public GH_PointCloudParam()
            : base(new GH_InstanceDescription("Point Cloud", "Cloud", "Represents a point cloud.", "Saiga", "Params"))
        { }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{FB119943-1DFC-4D12-B94C-192C8FC93CB6}");
            }
        }

        protected override GH_PointCloud InstantiateT()
        {
            return new GH_PointCloud();
        }

        public BoundingBox ClippingBox
        {
            get { return Preview_ComputeClippingBox(); }
        }

        bool _hidden;
        public bool Hidden
        {
            get { return _hidden; }
            set { _hidden = value; }
        }

        bool IGH_BakeAwareObject.IsBakeCapable
        {
            get { return !m_data.IsEmpty; }
        }

        bool IGH_PreviewObject.IsPreviewCapable
        {
            get { return true;  }
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_PointCloud> values)
        {
            var result = LoadFromSelectionPC(out List<PointCloud> pcs);
            if (result == GH_GetterResult.success)
            {
                values = pcs.Select(pc => new GH_PointCloud(pc)).ToList();
            }
            return result;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_PointCloud value)
        {
            var result = LoadFromSelection(out PointCloud pc);
            if (result == GH_GetterResult.success) {
                value = new GH_PointCloud(pc);
            }
            return result;
        }

        public GH_GetterResult LoadFromSelection(out PointCloud pc)
        {
            var go = new GetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Point | Rhino.DocObjects.ObjectType.PointSet;
            if (go.GetMultiple(1, 0) == Rhino.Input.GetResult.Cancel)
            {
                pc = null;
                return GH_GetterResult.cancel;
            }
            pc = new PointCloud();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                var obj = go.Object(i);
                var rhObj = obj.Object();
                if (rhObj.ObjectType == ObjectType.Point)
                {
                    var pt = obj.Point().Location;
                    var col = rhObj.Attributes.ObjectColor;
                    pc.Add(pt, col);
                }
                else if (rhObj.ObjectType == ObjectType.PointSet)
                {
                    using (PointCloud cloud = obj.PointCloud())
                    {
                        foreach (var item in cloud.AsEnumerable())
                        {
                            pc.Add(item.Location, item.Normal, item.Color);
                        }
                    }
                }
            }
            return GH_GetterResult.success;
        }

        public GH_GetterResult LoadFromSelectionPC(out List<PointCloud> pcs)
        {
            var go = new GetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.PointSet;
            if (go.GetMultiple(1, 0) == Rhino.Input.GetResult.Cancel)
            {
                pcs = null;
                return GH_GetterResult.cancel;
            }
            pcs = new List<PointCloud>();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                var obj = go.Object(i);
                var rhObj = obj.Object();
                 if (rhObj.ObjectType == ObjectType.PointSet)
                {
                    pcs.Add(obj.PointCloud());
                }
            }
            return GH_GetterResult.success;
        }
        void IGH_BakeAwareObject.BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, null, obj_ids);
        }

        public void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (att == null)
            {
                att = doc.CreateDefaultAttributes();
            }
            foreach (IGH_BakeAwareObject item in m_data)
            {
                if (item != null)
                {
                    List<Guid> idsOut = new List<Guid>();
                    item.BakeGeometry(doc, att, idsOut);
                    obj_ids.AddRange(idsOut);
                }
            }
        }

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // No meshes
        }

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args)
        {
            Preview_DrawWires(args);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.icon_pointcloudparam;
            }
        }
    }
}

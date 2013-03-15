using System.IO;
using System.Xml;
using Forge.Framework;
using Forge.Framework.Draw;
using Forge.Framework.UI;
using Forge.Framework.UI.Components;
using Microsoft.Xna.Framework;

namespace Forge.Core.HullEditor {

    #region namespace target stuff

    internal delegate void TranslateDragToExtern(object caller, ref float dx, ref float dy, bool doClampCheck);

    //internal delegate void RecieveDragFromExtern(HandleAlias handleAlias, ref float dx, ref float dy, bool doApplyChange);

    internal enum HandleAlias {
        First,
        Middle,
        Last,
        ExtremaY //the handle this alias cooresponds to changes depending on which handle has the highest Y value
    }

    internal enum PanelAlias {
        Side,
        Top,
        Back
    }

    #endregion

    #region abstract target class

    internal abstract class HullEditorPanel {
        protected readonly Button Background;
        protected readonly FloatingRectangle BoundingBox;
        public BezierCurveCollection Curves;
        protected RenderTarget RenderTarget;

        protected HullEditorPanel(int x, int y, int width, int height, string defaultCurveConfiguration, PanelAlias panelType) {
            BoundingBox = new FloatingRectangle(x, y, width, height);
            RenderTarget = new RenderTarget();
            RenderTarget.Bind();
            Curves = new BezierCurveCollection(
                target: RenderTarget,
                defaultConfig: defaultCurveConfiguration,
                areaToFill: new FloatingRectangle(
                    x + width * 0.1f,
                    y + height * 0.1f,
                    width - width * 0.2f,
                    height - height * 0.2f
                    ),
                panelType: panelType
                );
            UIElementCollection.BoundCollection.AddDragConstraintCallback(ClampChildElements);
            Background =
                new Button(
                    x: x,
                    y: y,
                    width: width,
                    height: height,
                    depth: UIElementCollection.BoundCollection.GetAbsoluteDepth(DepthLevel.Background),
                    textureName: "Materials/BlueBox",
                    spriteTexRepeatX: width / (Curves.PixelsPerMeter * 1),
                    spriteTexRepeatY: height / (Curves.PixelsPerMeter * 1),
                    components: new IUIComponent[] { new PanelComponent() }
                    );
            Update();
            RenderTarget.Unbind();
        }

        protected abstract void DisposeChild();

        public void SaveCurves(string fileName) {
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            Stream outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            var writer = XmlWriter.Create(outputStream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Data");
            writer.WriteElementString("NumControllers", null, Curves.Count.ToString());

            for (int i = 0; i < Curves.Count; i++) {
                writer.WriteStartElement("Handle" + i, null);
                writer.WriteElementString("PosX", null, ((Curves[i].CenterHandlePos.X - Curves.MinX) / Curves.PixelsPerMeter).ToString());
                writer.WriteElementString("PosY", null, ((Curves[i].CenterHandlePos.Y - Curves.MinY) / Curves.PixelsPerMeter).ToString());
                writer.WriteElementString("Angle", null, Curves[i].Handle.Angle.ToString());
                writer.WriteElementString("PrevLength", null, (Curves[i].PrevHandleLength / Curves.PixelsPerMeter).ToString());
                writer.WriteElementString("NextLength", null, (Curves[i].NextHandleLength / Curves.PixelsPerMeter).ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();
            outputStream.Close();
        }

        void ClampChildElements(IUIInteractiveElement owner, ref int x, ref int y, int oldX, int oldY) {
            if (x > BoundingBox.X + BoundingBox.Width || x < BoundingBox.X) {
                x = oldX;
            }
            if (y > BoundingBox.Y + BoundingBox.Height || y < BoundingBox.Y) {
                y = oldY;
            }
        }

        public void Draw() {
            RenderTarget.Draw(new Matrix(), Color.Transparent);
        }

        public void Dispose() {
            RenderTarget.Dispose();
        }

        public void Update() {
            Curves.Update();
        }

        /// <summary>
        ///   this function accepts modifications in METERS
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="dx"> </param>
        /// <param name="dy"> </param>
        /// <param name="doClampCheck"> </param>
        public void ModifyHandlePosition(HandleAlias handle, ref float dx, ref float dy, bool doClampCheck) {
            var dxi = (dx * Curves.PixelsPerMeter);
            var dyi = (dy * Curves.PixelsPerMeter);

            if (!doClampCheck) {
                switch (handle) {
                    case HandleAlias.First:
                        Curves[0].Handle.TranslatePosition(dxi, dyi);
                        break;
                    case HandleAlias.Middle:
                        Curves[Curves.Count / 2].Handle.TranslatePosition(dxi, dyi);
                        break;
                    case HandleAlias.Last:
                        Curves[Curves.Count - 1].Handle.TranslatePosition(dxi, dyi);
                        break;
                    case HandleAlias.ExtremaY:
                        var extremaController = Curves[0];
                        foreach (var curve in Curves) {
                            if (curve.CenterHandlePos.Y > extremaController.CenterHandlePos.Y) {
                                extremaController = curve;
                            }
                        }
                        extremaController.Handle.TranslatePosition(dxi, dyi);
                        break;
                }
            }
            else {
            }
        }

        protected abstract void ProcExternalDrag(object caller, ref float dx, ref float dy, bool doApplyChange);
    }

    #endregion

    #region sidepanel impl

    internal class SideEditorPanel : HullEditorPanel {
        public BackEditorPanel BackPanel;
        public TopEditorPanel TopPanel;

        public SideEditorPanel(int x, int y, int width, int height, string defaultCurveConfiguration)
            : base(x, y, width, height, defaultCurveConfiguration, PanelAlias.Side) {
            foreach (var curve in Curves) {
                curve.Handle.TranslateToExtern = ProcExternalDrag;
            }
        }

        protected override void DisposeChild() {
            BackPanel = null;
            TopPanel = null;
        }

        protected override void ProcExternalDrag(object caller, ref float dx, ref float dy, bool applyClampCheck) {
            float dxf = dx / Curves.PixelsPerMeter;
            float dyf = dy / Curves.PixelsPerMeter;

            var controller = (CurveHandle)caller;
            float _null = 0;

            //Curves[0] is the frontmost controller that represents the limit of the bow
            if (controller == Curves[0].Handle) {
                if (TopPanel != null) {
                    TopPanel.ModifyHandlePosition(HandleAlias.Middle, ref dxf, ref _null, applyClampCheck);
                }
                if (BackPanel != null) {
                    BackPanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dyf, applyClampCheck);
                    BackPanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dyf, applyClampCheck);
                }
            }

            //Curves[Curves.Count-1] is the hindmost controller that represents the limit of the stern
            if (controller == Curves[Curves.Count - 1].Handle) {
                if (TopPanel != null) {
                    TopPanel.ModifyHandlePosition(HandleAlias.Last, ref dxf, ref _null, applyClampCheck);
                    TopPanel.ModifyHandlePosition(HandleAlias.First, ref dxf, ref _null, applyClampCheck);
                }
                if (BackPanel != null) {
                    BackPanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dyf, applyClampCheck);
                    BackPanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dyf, applyClampCheck);
                }
            }

            if (controller == Curves.MaxYCurve.Handle) {
                if (BackPanel != null) {
                    BackPanel.ModifyHandlePosition(HandleAlias.Middle, ref _null, ref dyf, applyClampCheck);
                }
            }
        }
    }

    #endregion

    #region toppanel impl

    internal class TopEditorPanel : HullEditorPanel {
        public BackEditorPanel BackPanel;
        public SideEditorPanel SidePanel;

        public TopEditorPanel(int x, int y, int width, int height, string defaultCurveConfiguration)
            : base(x, y, width, height, defaultCurveConfiguration, PanelAlias.Top) {
            Curves[0].Handle.TranslateToExtern = ProcExternalDrag;
            Curves[Curves.Count - 1].Handle.TranslateToExtern = ProcExternalDrag;
            Curves[Curves.Count / 2].Handle.TranslateToExtern = ProcExternalDrag;
        }

        protected override void DisposeChild() {
            BackPanel = null;
            SidePanel = null;
        }

        protected override void ProcExternalDrag(object caller, ref float dx, ref float dy, bool applyClampCheck) {
            float dxf = dx / Curves.PixelsPerMeter;
            float dyf = dy / Curves.PixelsPerMeter;

            float _null = 0;

            var controller = (CurveHandle)caller;
            if (controller == Curves[0].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.Last, ref dxf, ref _null, applyClampCheck);
                }
                if (BackPanel != null) {
                    dyf *= -1;
                    BackPanel.ModifyHandlePosition(HandleAlias.Last, ref dyf, ref _null, applyClampCheck);
                    dyf *= -1;
                    BackPanel.ModifyHandlePosition(HandleAlias.First, ref dyf, ref _null, applyClampCheck);
                }
            }
            if (controller == Curves[Curves.Count - 1].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.Last, ref dxf, ref _null, applyClampCheck);
                }
                if (BackPanel != null) {
                    BackPanel.ModifyHandlePosition(HandleAlias.Last, ref dyf, ref _null, applyClampCheck);
                    dyf *= -1;
                    BackPanel.ModifyHandlePosition(HandleAlias.First, ref dyf, ref _null, applyClampCheck);
                    dyf *= -1;
                }
            }
            if (controller == Curves[Curves.Count / 2].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.First, ref dxf, ref _null, applyClampCheck);
                }
            }
        }
    }

    #endregion

    #region backpanel impl

    internal class BackEditorPanel : HullEditorPanel {
        public SideEditorPanel SidePanel;
        public TopEditorPanel TopPanel;

        public BackEditorPanel(int x, int y, int width, int height, string defaultCurveConfiguration)
            : base(x, y, width, height, defaultCurveConfiguration, PanelAlias.Back) {
            Curves[0].Handle.TranslateToExtern = ProcExternalDrag;
            Curves[Curves.Count - 1].Handle.TranslateToExtern = ProcExternalDrag;
            Curves[Curves.Count / 2].Handle.TranslateToExtern = ProcExternalDrag;
        }

        protected override void DisposeChild() {
            SidePanel = null;
            TopPanel = null;
        }

        protected override void ProcExternalDrag(object caller, ref float dx, ref float dy, bool applyClampCheck) {
            float dxf = dx / Curves.PixelsPerMeter;
            float dyf = dy / Curves.PixelsPerMeter;

            float _null = 0;
            var controller = (CurveHandle)caller;
            if (controller == Curves[0].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dyf, applyClampCheck);
                    SidePanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dyf, applyClampCheck);
                }
                if (TopPanel != null) {
                    TopPanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dxf, applyClampCheck);
                    dxf *= -1;
                    TopPanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dxf, applyClampCheck);
                    dxf *= -1;
                }
            }
            if (controller == Curves[Curves.Count - 1].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dyf, applyClampCheck);
                    SidePanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dyf, applyClampCheck);
                }
                if (TopPanel != null) {
                    dxf *= -1;
                    TopPanel.ModifyHandlePosition(HandleAlias.First, ref _null, ref dxf, applyClampCheck);
                    dxf *= -1;
                    TopPanel.ModifyHandlePosition(HandleAlias.Last, ref _null, ref dxf, applyClampCheck);
                }
            }
            if (controller == Curves[Curves.Count / 2].Handle) {
                if (SidePanel != null) {
                    SidePanel.ModifyHandlePosition(HandleAlias.ExtremaY, ref _null, ref dyf, applyClampCheck);
                }
            }
        }
    }

    #endregion
}

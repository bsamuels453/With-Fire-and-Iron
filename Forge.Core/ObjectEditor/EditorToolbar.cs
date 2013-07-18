#region

using Forge.Core.ObjectEditor.Tools;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Forge.Framework.UI.Elements;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor{
    internal class EditorToolbar : ElementGrid{
        const string _template = "UiTemplates/Specialized/ObjectEditorToolbar.json";

        readonly ToolbarButton _buildLadderBut;
        readonly ToolbarButton _buildWallBut;
        readonly ToolbarButton _deleteWallBut;
        readonly IToolbarTool[] _tools;

        public EditorToolbar(HullDataManager hullData, UIElementCollection parent, FrameStrata.Level depth, Point position) :
            base(parent, depth, position, _template){
            var jobj = Resource.LoadJObject(_template);
            var wallBuildTex = jobj["BuildWallTex"].ToObject<string>();
            var wallDeleteTex = jobj["DeleteWallTex"].ToObject<string>();
            var ladderTex = jobj["BuildLadderTex"].ToObject<string>();

            _buildWallBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), wallBuildTex);
            _deleteWallBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), wallDeleteTex);
            _buildLadderBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), ladderTex);

            base.AddGridElement(_buildWallBut, 0, 0);
            base.AddGridElement(_deleteWallBut, 0, 1);
            base.AddGridElement(_buildLadderBut, 0, 2);

            _tools = new IToolbarTool[3];

            _tools[(int) Tools.BuildWall] = new WallBuildTool(hullData);
            _tools[(int) Tools.DeleteWall] = new WallDeleteTool(hullData);
            _tools[(int) Tools.BuildLadder] = new LadderBuildTool(hullData);

            InitializeToolEvents();
        }

        void InitializeToolEvents(){
            _buildWallBut.OnLeftClick +=
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        foreach (var tool in _tools){
                            tool.Enabled = false;
                        }
                        _tools[(int) Tools.BuildWall].Enabled = true;
                    }
                };
            _deleteWallBut.OnLeftClick +=
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        foreach (var tool in _tools){
                            tool.Enabled = false;
                        }
                        _tools[(int) Tools.DeleteWall].Enabled = true;
                    }
                };
            _buildLadderBut.OnLeftClick +=
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        foreach (var tool in _tools){
                            tool.Enabled = false;
                        }
                        _tools[(int) Tools.BuildLadder].Enabled = true;
                    }
                };
        }

        public void DisposeTools(){
            foreach (var tool in _tools){
                tool.Dispose();
            }
        }

        #region Nested type: Tools

        enum Tools{
            BuildWall,
            DeleteWall,
            BuildLadder
        }

        #endregion
    }
}
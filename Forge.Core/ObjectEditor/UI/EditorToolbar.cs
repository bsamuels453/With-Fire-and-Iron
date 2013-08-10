#region

using Forge.Core.GameObjects;
using Forge.Core.ObjectEditor.Tools;
using Forge.Framework.Resources;
using Forge.Framework.UI;
using Forge.Framework.UI.Elements;
using MonoGameUtility;

#endregion

namespace Forge.Core.ObjectEditor.UI{
    internal class EditorToolbar : ElementGrid{
        const string _template = "UiTemplates/Specialized/ObjectEditorToolbar.json";
        readonly ToolbarButton _buildCannonBut;
        readonly ToolbarButton _buildEngineBut;

        readonly ToolbarButton _buildLadderBut;
        readonly ToolbarButton _buildWallBut;
        readonly ToolbarButton _deleteWallBut;
        readonly IToolbarTool[] _tools;

        public EditorToolbar(
            HullEnvironment hullEnv,
            GameObjectEnvironment gameObjEnv,
            InternalWallEnvironment wallEnv,
            UIElementCollection parent,
            FrameStrata.Level depth,
            Point position) :
                base(parent, depth, position, _template){
            var jobj = Resource.LoadJObject(_template);
            var wallBuildTex = jobj["BuildWallTex"].ToObject<string>();
            var wallDeleteTex = jobj["DeleteWallTex"].ToObject<string>();
            var ladderTex = jobj["BuildLadderTex"].ToObject<string>();
            var cannonTex = jobj["BuildCannonTex"].ToObject<string>();
            var buildEngineTex = jobj["BuildEngineTex"].ToObject<string>();

            _buildWallBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), wallBuildTex);
            _deleteWallBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), wallDeleteTex);
            _buildLadderBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), ladderTex);
            _buildCannonBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), cannonTex);
            _buildEngineBut = new ToolbarButton(this, FrameStrata.Level.Medium, new Point(), buildEngineTex);

            base.AddGridElement(_buildWallBut, 0, 0);
            base.AddGridElement(_deleteWallBut, 0, 1);
            base.AddGridElement(_buildLadderBut, 0, 2);
            base.AddGridElement(_buildCannonBut, 0, 3);
            base.AddGridElement(_buildEngineBut, 0, 4);

            _tools = new IToolbarTool[5];

            _tools[(int) Tools.BuildWall] = new WallBuildTool(hullEnv, wallEnv);
            _tools[(int) Tools.DeleteWall] = new WallDeleteTool(hullEnv, wallEnv);
            _tools[(int) Tools.BuildLadder] = new DeckObjectPlacementTool
                (
                hullEnv,
                gameObjEnv,
                0,
                GameObjectFamily.Ladders,
                ""
                );
            _tools[(int) Tools.BuildCannon] = new DeckObjectPlacementTool
                (
                hullEnv,
                gameObjEnv,
                0,
                GameObjectFamily.Cannons,
                "0"
                );
            _tools[(int) Tools.BuildEngine] = new ZoningTool(hullEnv, gameObjEnv, new EngineGenerator());

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

            _buildCannonBut.OnLeftClick +=
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        foreach (var tool in _tools){
                            tool.Enabled = false;
                        }
                        _tools[(int) Tools.BuildCannon].Enabled = true;
                    }
                };
            _buildEngineBut.OnLeftClick +=
                (state, f, arg3) =>{
                    if (arg3.ContainsMouse){
                        foreach (var tool in _tools){
                            tool.Enabled = false;
                        }
                        _tools[(int) Tools.BuildEngine].Enabled = true;
                    }
                };
        }

        public override void Dispose(){
            foreach (var tool in _tools){
                tool.Dispose();
            }
            base.Dispose();
        }

        #region Nested type: Tools

        enum Tools{
            BuildWall,
            DeleteWall,
            BuildLadder,
            BuildCannon,
            BuildEngine
        }

        #endregion
    }
}
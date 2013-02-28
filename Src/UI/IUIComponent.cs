#region

using Gondola.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Gondola.UI{
    internal interface IUIComponent {
        /// <summary>
        ///   Disabling a component will cause it to ignore all public method calls, and ignore all event dispatches. Enabling a component will undo these changes. Components start enabled by default.
        /// </summary>
        bool Enabled { get; set; }

        string Identifier { get; }

        /// <summary>
        ///   A reference to the owner of the element.
        /// </summary>
        void ComponentCtor(IUIElement owner, ButtonEventDispatcher ownerEventDispatcher);

        /// <summary>
        ///   An update function that will be called by the component's owner element.
        /// </summary>
        void Update();
    }
}
using Aga.Controls.Tree.NodeControls;
using System.Drawing;

namespace Aga.Controls.Tree
{
	public struct NodeControlInfo
	{
		public static readonly NodeControlInfo Empty = new NodeControlInfo(null, Rectangle.Empty, null);

        public NodeControl Control { get; }
        public Rectangle Bounds { get; }
        public TreeNodeAdv Node { get; }

        public NodeControlInfo(NodeControl control, Rectangle bounds, TreeNodeAdv node)
		{
			Control = control;
			Bounds = bounds;
			Node = node;
		}
	}
}

using System;

namespace Aga.Controls.Tree
{
	public abstract class TreeModelBase: ITreeModel
	{
		public abstract System.Collections.IEnumerable GetChildren(TreePath treePath);
		public abstract bool IsLeaf(TreePath treePath);


		public event EventHandler<TreeModelEventArgs> NodesChanged;
		protected void OnNodesChanged(TreeModelEventArgs args)
        {
            NodesChanged?.Invoke(this, args);
        }

		public event EventHandler<TreePathEventArgs> StructureChanged;
		protected void OnStructureChanged(TreePathEventArgs args)
        {
            StructureChanged?.Invoke(this, args);
        }

		public event EventHandler<TreeModelEventArgs> NodesInserted;
		protected void OnNodesInserted(TreeModelEventArgs args)
        {
            NodesInserted?.Invoke(this, args);
        }

		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		protected void OnNodesRemoved(TreeModelEventArgs args)
        {
            NodesRemoved?.Invoke(this, args);
        }

		public virtual void Refresh()
		{
			OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
		}
	}
}

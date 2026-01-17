using System;

namespace Aga.Controls.Tree
{
	/// <summary>
	/// Converts IEnumerable interface to ITreeModel. 
	/// Allows to display a plain list in the TreeView
	/// </summary>
	public class TreeListAdapter : ITreeModel
	{
		private System.Collections.IEnumerable _list;

		public TreeListAdapter(System.Collections.IEnumerable list)
		{
			_list = list;
		}

		#region ITreeModel Members

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
        {
            if (treePath.IsEmpty())
				return _list;
            return null;
        }

		public bool IsLeaf(TreePath treePath)
		{
			return true;
		}

		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public void OnNodesChanged(TreeModelEventArgs args)
        {
            NodesChanged?.Invoke(this, args);
        }

		public event EventHandler<TreePathEventArgs> StructureChanged;
		public void OnStructureChanged(TreePathEventArgs args)
        {
            StructureChanged?.Invoke(this, args);
        }

		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public void OnNodeInserted(TreeModelEventArgs args)
        {
            NodesInserted?.Invoke(this, args);
        }

		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public void OnNodeRemoved(TreeModelEventArgs args)
        {
            NodesRemoved?.Invoke(this, args);
        }

		#endregion
	}
}

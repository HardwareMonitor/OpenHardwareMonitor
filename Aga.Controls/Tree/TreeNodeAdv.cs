using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Aga.Controls.Tree
{
	[Serializable]
	public sealed class TreeNodeAdv : ISerializable
	{
		#region NodeCollection

		private class NodeCollection : Collection<TreeNodeAdv>
		{
			private readonly TreeNodeAdv _owner;

			public NodeCollection(TreeNodeAdv owner) => _owner = owner;

            protected override void ClearItems()
			{
				while (Count != 0)
					RemoveAt(Count - 1);
			}

			protected override void InsertItem(int index, TreeNodeAdv item)
			{
				if (item == null)
					throw new ArgumentNullException(nameof(item));

				if (item.Parent != _owner)
				{
                    item.Parent?.Nodes.Remove(item);
                    item.Parent = _owner;
					item.Index = index;
					for (int i = index; i < Count; i++)
						this[i].Index++;
					base.InsertItem(index, item);
				}

				if (_owner.Tree != null && _owner.Tree.Model == null)
				{
					_owner.Tree.SmartFullUpdate();
				}
			}

			protected override void RemoveItem(int index)
			{
				TreeNodeAdv item = this[index];
				item.Parent = null;
				item.Index = -1;
				for (int i = index + 1; i < Count; i++)
					this[i].Index--;
				base.RemoveItem(index);

				if (_owner.Tree != null && _owner.Tree.Model == null)
				{
					_owner.Tree.UpdateSelection();
					_owner.Tree.SmartFullUpdate();
				}
			}

			protected override void SetItem(int index, TreeNodeAdv item)
			{
				if (item == null)
					throw new ArgumentNullException(nameof(item));
				RemoveAt(index);
				InsertItem(index, item);
			}
		}

		#endregion

		#region Events

		public event EventHandler<TreeViewAdvEventArgs> Collapsing;
		internal void OnCollapsing() => Collapsing?.Invoke(this, new TreeViewAdvEventArgs(this));

        public event EventHandler<TreeViewAdvEventArgs> Collapsed;
		internal void OnCollapsed(bool byUser) => Collapsed?.Invoke(this, new TreeViewAdvEventArgs(this, byUser));

        public event EventHandler<TreeViewAdvEventArgs> Expanding;
		internal void OnExpanding() => Expanding?.Invoke(this, new TreeViewAdvEventArgs(this));

        public event EventHandler<TreeViewAdvEventArgs> Expanded;
		internal void OnExpanded(bool byUser) => Expanded?.Invoke(this, new TreeViewAdvEventArgs(this, byUser));

        #endregion

		#region Properties

        public TreeViewAdv Tree { get; }
		public int Row { get; internal set; }
        public int Index { get; private set; } = -1;

        private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
            {
                if (_isSelected == value)
                    return;

                if (Tree.IsMyNode(this))
                {
                    //_tree.OnSelectionChanging
                    if (value)
                    {
                        if (!Tree.Selection.Contains(this))
                            Tree.Selection.Add(this);

                        if (Tree.Selection.Count == 1)
                            Tree.CurrentNode = this;
                    }
                    else
                        Tree.Selection.Remove(this);
                    Tree.UpdateView();
                    Tree.OnSelectionChanged();
                }
                _isSelected = value;
            }
		}

		/// <summary>
		/// Returns true if all parent nodes of this node are expanded.
		/// </summary>
		internal bool IsVisible
		{
			get
			{
				TreeNodeAdv node = Parent;
				while (node != null)
				{
					if (!node.IsExpanded)
						return false;
					node = node.Parent;
				}
				return true;
			}
		}

		public bool IsLeaf { get; internal set; }
		public bool IsExpandedOnce { get; internal set; }
        public bool IsExpanded { get; private set; }
        internal void AssignIsExpanded(bool value) => IsExpanded = value;
        public TreeNodeAdv Parent { get; private set; }
		public int Level => Parent?.Level + 1 ?? 0;

        public TreeNodeAdv PreviousNode
		{
			get
			{
                TreeNodeAdv parent = Parent;
                if (parent?.Nodes != null)
				{
					int index = Index;
					if (index > 0)
						return parent.Nodes[index - 1];
				}
				return null;
			}
		}

		public TreeNodeAdv NextNode
		{
			get
			{
                var nodes = Parent?.Nodes;
                int index = Index;
                if (nodes != null && index < nodes.Count - 1)
                    return nodes[index + 1]; //todo: add lock to be thread safe
				return null;
			}
		}

		internal TreeNodeAdv BottomNode
		{
			get
            {
                TreeNodeAdv parent = Parent;
                return parent != null ? parent.NextNode ?? parent.BottomNode : null;
            }
		}

		internal TreeNodeAdv NextVisibleNode
		{
            get
            {
                var firstNode = Nodes.FirstOrDefault();
                return IsExpanded && firstNode != null ? firstNode : NextNode ?? BottomNode;
            }
		}

		public bool CanExpand => Nodes.Count > 0 || (!IsExpandedOnce && !IsLeaf);

        public object Tag { get; private set; }
		internal Collection<TreeNodeAdv> Nodes { get; private set; }
		public ReadOnlyCollection<TreeNodeAdv> Children { get; private set; }
		internal int? RightBounds { get; set; }
		internal int? Height { get; set; }
		internal bool IsExpandingNow { get; set; }
        public bool AutoExpandOnStructureChanged { get; set; } = true;

		#endregion

		public TreeNodeAdv(object tag)
			: this(null, tag)
		{
		}

		internal TreeNodeAdv(TreeViewAdv tree, object tag)
		{
            Row = -1;
			Tree = tree;
            Nodes = new NodeCollection(this);
            Children = new ReadOnlyCollection<TreeNodeAdv>(Nodes);
			Tag = tag;
		}

		public override string ToString() => Tag != null ? Tag.ToString() : base.ToString();

        public void Collapse(bool raisedByUser)
		{
			if (IsExpanded)
				Collapse(true, raisedByUser);
		}

		public void CollapseAll() => Collapse(false, false);
        public void Collapse(bool ignoreChildren, bool raisedByUser) => SetIsExpanded(false, ignoreChildren);

        public void Expand(bool raisedByUser)
		{
			if (!IsExpanded)
				Expand(true, raisedByUser);
		}

		public void ExpandAll() => Expand(false, false);
        public void Expand(bool ignoreChildren, bool raisedByUser) => SetIsExpanded(true, ignoreChildren);

        private void SetIsExpanded(bool value, bool ignoreChildren)
		{
			if (Tree == null)
				IsExpanded = value;
			else
				Tree.SetIsExpanded(this, value, ignoreChildren);
		}

		#region ISerializable Members

		private TreeNodeAdv(SerializationInfo info, StreamingContext context)
			: this(null, null)
		{
            int nodesCount = info.GetInt32("NodesCount");
			IsExpanded = info.GetBoolean("IsExpanded");
			Tag = info.GetValue("Tag", typeof(object));

			for (int i = 0; i < nodesCount; i++)
			{
				TreeNodeAdv child = (TreeNodeAdv)info.GetValue("Child" + i, typeof(TreeNodeAdv));
				Nodes.Add(child);
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("IsExpanded", IsExpanded);
			info.AddValue("NodesCount", Nodes.Count);
			if (Tag != null && Tag.GetType().IsSerializable)
				info.AddValue("Tag", Tag, Tag.GetType());

			for (int i = 0; i < Nodes.Count; i++)
				info.AddValue("Child" + i, Nodes[i], typeof(TreeNodeAdv));
		}

		#endregion
	}
}

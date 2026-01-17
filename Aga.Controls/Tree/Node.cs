using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;

namespace Aga.Controls.Tree
{
	public class Node
	{
		#region NodeCollection

		private class NodeCollection : Collection<Node>
		{
			private Node _owner;

			public NodeCollection(Node owner)
			{
				_owner = owner;
			}

			protected override void ClearItems()
			{
				while (Count != 0)
					RemoveAt(Count - 1);
			}

			protected override void InsertItem(int index, Node item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				if (item.Parent != _owner)
				{
                    item.Parent?.Nodes.Remove(item);
                    item._parent = _owner;
					item.Index = index;
					for (int i = index; i < Count; i++)
						this[i].Index++;
					base.InsertItem(index, item);

					TreeModel model = _owner.FindModel();
                    model?.OnNodeInserted(_owner, index, item);
                }
			}

			protected override void RemoveItem(int index)
			{
				Node item = this[index];
				item._parent = null;
				item.Index = -1;
				for (int i = index + 1; i < Count; i++)
					this[i].Index--;
				base.RemoveItem(index);

				TreeModel model = _owner.FindModel();
                model?.OnNodeRemoved(_owner, index, item);
            }

			protected override void SetItem(int index, Node item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				RemoveAt(index);
				InsertItem(index, item);
			}
		}

		#endregion

		#region Properties

        internal TreeModel Model { get; set; }

        private NodeCollection _nodes;
		public Collection<Node> Nodes
		{
			get { return _nodes; }
		}

		private Node _parent;
		public Node Parent
		{
			get { return _parent; }
			set
			{
				if (value != _parent)
				{
                    _parent?.Nodes.Remove(this);
                    value?.Nodes.Add(this);
                }
			}
		}

        public int Index { get; private set; } = -1;

        public Node PreviousNode
		{
			get
            {
                int index = Index;
                return index > 0 ? _parent.Nodes[index - 1] : null;
            }
		}

		public Node NextNode
		{
			get
            {
                int index = Index;
                return index >= 0 && index < _parent.Nodes.Count - 1 ? _parent.Nodes[index + 1] : null;
            }
		}

		private string _text;
		public virtual string Text
		{
			get { return _text; }
			set
            {
                if (_text == value)
                    return;
                _text = value;
                NotifyModel();
            }
		}

		private CheckState _checkState;
		public virtual CheckState CheckState
		{
			get { return _checkState; }
			set
			{
				if (_checkState != value)
				{
					_checkState = value;
					NotifyModel();
				}
			}
		}

		private Image _image;
		public Image Image
		{
			get { return _image; }
			set
			{
				if (_image != value)
				{
					_image = value;
					NotifyModel();
				}
			}
		}

        public object Tag { get; set; }

        public bool IsChecked
		{
			get
			{
				return CheckState != CheckState.Unchecked;
			}
			set
			{
				if (value)
					CheckState = CheckState.Checked;
				else
					CheckState = CheckState.Unchecked;
			}
		}

		public virtual bool IsLeaf
		{
			get
			{
				return false;
			}
		}

		#endregion

		public Node()
			: this(string.Empty)
		{
		}

		public Node(string text)
		{
			_text = text;
			_nodes = new NodeCollection(this);
		}

		public override string ToString()
		{
			return Text;
		}

		private TreeModel FindModel()
		{
			Node node = this;
			while (node != null)
			{
				if (node.Model != null)
					return node.Model;
				node = node.Parent;
			}
			return null;
		}

		protected void NotifyModel()
		{
			TreeModel model = FindModel();
			if (model != null && Parent != null)
			{
				TreePath path = model.GetPath(Parent);
				if (path != null)
				{
					TreeModelEventArgs args = new TreeModelEventArgs(path, new int[] { Index }, new object[] { this });
					model.OnNodesChanged(args);
				}
			}
		}
	}
}

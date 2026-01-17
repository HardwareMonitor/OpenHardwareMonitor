using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using System.Collections;

using Aga.Controls.Tree.NodeControls;
using Aga.Controls.Threading;

namespace Aga.Controls.Tree
{
	/// <summary>
	/// Extensible advanced <see cref="TreeView"/> implemented in 100% managed C# code.
	/// Features: Model/View architecture. Multiple column per node. Ability to select
	/// multiple tree nodes. Different types of controls for each node column:
	/// <see cref="CheckBox"/>, Icon, Label... Drag and Drop highlighting. Load on
	/// demand of nodes. Incremental search of nodes.
	/// </summary>
	public partial class TreeViewAdv : Control
	{
        public static Action<Graphics, Rectangle, bool> CustomCheckRenderFunc;
        public static Action<Graphics, Rectangle, bool, bool> CustomColumnBackgroundRenderFunc;
        public static Action<Graphics, Rectangle, Font, string> CustomColumnTextRenderFunc;
        public static Color CustomSelectedTextColor = SystemColors.ControlText;
        public static Pen CustomHorizontalLinePen = new Pen(Color.FromArgb(247, 247, 247));
        public static Action<Graphics, Rectangle, bool> CustomPlusMinusRenderFunc;
        public static Brush CustomSelectedRowBrush = new SolidBrush(Color.FromArgb(240, 240, 240));

        private const int LeftMargin = 7;
		internal const int ItemDragSensivity = 4;
		private readonly int _columnHeaderHeight;
		private const int DividerWidth = 9;
		private const int DividerCorrectionGap = -2;

		private Pen _linePen;
		private Pen _markPen;
		private bool _suspendUpdate;
		private bool _needFullUpdate;
		private bool _fireSelectionEvent;
		private NodePlusMinus _plusMinus;
		private ToolTip _toolTip;
		private DrawContext _measureContext;
		private TreeColumn _hotColumn;
		private IncrementalSearch _search;
		private List<TreeNodeAdv> _expandingNodes = new List<TreeNodeAdv>();
		private AbortableThreadPool _threadPool = new AbortableThreadPool();

		private float dpiX;
		private float dpiY;
		private float dpiXscale = 1;
		private float dpiYscale = 1;

		#region Public Events

		[Category("Action")]
		public event ItemDragEventHandler ItemDrag;
		private void OnItemDrag(MouseButtons buttons, object item) => ItemDrag?.Invoke(this, new ItemDragEventArgs(buttons, item));

        [Category("Behavior")]
		public event EventHandler<TreeNodeAdvMouseEventArgs> NodeMouseClick;
		private void OnNodeMouseClick(TreeNodeAdvMouseEventArgs args) => NodeMouseClick?.Invoke(this, args);

        [Category("Behavior")]
		public event EventHandler<TreeNodeAdvMouseEventArgs> NodeMouseDoubleClick;
		private void OnNodeMouseDoubleClick(TreeNodeAdvMouseEventArgs args) => NodeMouseDoubleClick?.Invoke(this, args);

        [Category("Behavior")]
		public event EventHandler<TreeColumnEventArgs> ColumnWidthChanged;
		internal void OnColumnWidthChanged(TreeColumn column) => ColumnWidthChanged?.Invoke(this, new TreeColumnEventArgs(column));

        [Category("Behavior")]
		public event EventHandler<TreeColumnEventArgs> ColumnReordered;
		internal void OnColumnReordered(TreeColumn column) => ColumnReordered?.Invoke(this, new TreeColumnEventArgs(column));

        [Category("Behavior")]
		public event EventHandler<TreeColumnEventArgs> ColumnClicked;
		internal void OnColumnClicked(TreeColumn column) => ColumnClicked?.Invoke(this, new TreeColumnEventArgs(column));

        [Category("Behavior")]
		public event EventHandler SelectionChanged;
		internal void OnSelectionChanged()
		{
			if (SuspendSelectionEvent)
				_fireSelectionEvent = true;
			else
			{
				_fireSelectionEvent = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
		}

		[Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Collapsing;
		private void OnCollapsing(TreeNodeAdv node) => Collapsing?.Invoke(this, new TreeViewAdvEventArgs(node));

        [Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Collapsed;
		private void OnCollapsed(TreeNodeAdv node, bool byUser) => Collapsed?.Invoke(this, new TreeViewAdvEventArgs(node, byUser));

        [Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Expanding;
		private void OnExpanding(TreeNodeAdv node) => Expanding?.Invoke(this, new TreeViewAdvEventArgs(node));

        [Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Expanded;
		private void OnExpanded(TreeNodeAdv node, bool byUser) => Expanded?.Invoke(this, new TreeViewAdvEventArgs(node, byUser));

        [Category("Behavior")]
		public event EventHandler GridLineStyleChanged;
		private void OnGridLineStyleChanged() => GridLineStyleChanged?.Invoke(this, EventArgs.Empty);

        [Category("Behavior")]
		public event ScrollEventHandler Scroll;
		protected virtual void OnScroll(ScrollEventArgs e) => Scroll?.Invoke(this, e);

        [Category("Behavior")]
		public event EventHandler<TreeViewRowDrawEventArgs> RowDraw;
		protected virtual void OnRowDraw(PaintEventArgs e, TreeNodeAdv node, DrawContext context, int row, Rectangle rowRect)
        {
            if (RowDraw == null)
                return;

            TreeViewRowDrawEventArgs args = new TreeViewRowDrawEventArgs(e.Graphics, e.ClipRectangle, node, context, row, rowRect);
            RowDraw(this, args);
        }

		/// <summary>
		/// Fires when control is going to draw. Can be used to change text or back color
		/// </summary>
		[Category("Behavior")]
		public event EventHandler<DrawEventArgs> DrawControl;
		internal bool DrawControlMustBeFired() => DrawControl != null;
        internal void FireDrawControl(DrawEventArgs args) => OnDrawControl(args);
        protected virtual void OnDrawControl(DrawEventArgs args) => DrawControl?.Invoke(this, args);

        [Category("Drag Drop")]
		public event EventHandler<DropNodeValidatingEventArgs> DropNodeValidating;
		protected virtual void OnDropNodeValidating(Point point, ref TreeNodeAdv node)
        {
            if (DropNodeValidating == null)
                return;
            DropNodeValidatingEventArgs args = new DropNodeValidatingEventArgs(point, node);
            DropNodeValidating(this, args);
            node = args.Node;
        }

		#endregion

		public TreeViewAdv()
		{
			InitializeComponent();
			SetDPI();
			SetStyle(ControlStyles.AllPaintingInWmPaint
				| ControlStyles.UserPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.ResizeRedraw
				| ControlStyles.Selectable
				, true);


			_columnHeaderHeight = Application.RenderWithVisualStyles ? 20 : 17;
			_columnHeaderHeight = GetScaledSize(_columnHeaderHeight);

			//BorderStyle = BorderStyle.Fixed3D;
			_hScrollBar.Height = SystemInformation.HorizontalScrollBarHeight;
			_vScrollBar.Width = SystemInformation.VerticalScrollBarWidth;
			_rowLayout = new FixedRowHeightLayout(this, RowHeight);
			RowMap = new List<TreeNodeAdv>();
			Selection = new List<TreeNodeAdv>();
			SelectedNodes = new ReadOnlyCollection<TreeNodeAdv>(Selection);
			_columns = new TreeColumnCollection(this);
			_toolTip = new ToolTip();

			_measureContext = new DrawContext();
			_measureContext.Font = Font;
			_measureContext.Graphics = Graphics.FromImage(new Bitmap(1, 1));

			Input = new NormalInputState(this);
			_search = new IncrementalSearch(this);
			CreateNodes();
			CreatePens();

			ArrangeControls();

			_plusMinus = new NodePlusMinus();
			_controls = new NodeControlsCollection(this);

			Font = _font;
			ExpandingIcon.IconChanged += ExpandingIconChanged;

            BackColor = Color.Black;
		}

		public void SetDPI()
		{
			// https://msdn.microsoft.com/en-us/library/windows/desktop/dn469266(v=vs.85).aspx
			const int _default_dpi = 96;
			Graphics g = CreateGraphics();

			try
			{
				dpiX = g.DpiX;
				dpiY = g.DpiY;
			}
			finally
			{
				g.Dispose();
			}
			if (dpiX > 0)
			{
				dpiXscale = dpiX / _default_dpi;
			}
			if (dpiY > 0)
			{
				dpiYscale = dpiY / _default_dpi;
			}
		}

		public int GetScaledSize(int size, bool useY = true)
		{
			int scaledSize = size;
			if (useY && dpiYscale > 1)
			{
				scaledSize = (int)(dpiYscale * size);
			}
			else if (dpiXscale > 1)
			{
				scaledSize = (int)(dpiXscale * size);
			}
			return scaledSize;
		}

        private void ExpandingIconChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated && !IsDisposed)
				BeginInvoke(new MethodInvoker(DrawIcons));
		}

		private void DrawIcons()
		{
			using (Graphics gr = Graphics.FromHwnd(Handle))
			{
				//Apply the same Graphics Transform logic as used in OnPaint.
				int y = 0;
				if (UseColumns)
				{
					y += ColumnHeaderHeight;
					if (Columns.Count == 0)
						return;
				}
				int firstRowY = _rowLayout.GetRowBounds(FirstVisibleRow).Y;
				y -= firstRowY;
				gr.ResetTransform();
				gr.TranslateTransform(-OffsetX, y);

				DrawContext context = new DrawContext();
				context.Graphics = gr;
				for (int i = 0; i < _expandingNodes.Count; i++)
				{
					foreach (NodeControlInfo item in GetNodeControls(_expandingNodes[i]))
					{
						if (item.Control is ExpandingIcon)
						{
							Rectangle bounds = item.Bounds;
							if (item.Node.Parent == null && UseColumns)
								bounds.Location = Point.Empty; // display root expanding icon at 0,0

							context.Bounds = bounds;
							item.Control.Draw(item.Node, context);
						}
					}
				}
			}
		}

		#region Public Methods

		public TreePath GetPath(TreeNodeAdv node)
        {
            if (node == Root)
				return TreePath.Empty;
            Stack<object> stack = new Stack<object>();
            while (node != Root && node != null)
            {
                stack.Push(node.Tag);
                node = node.Parent;
            }
            return new TreePath(stack.ToArray());
        }

		public TreeNodeAdv GetNodeAt(Point point)
		{
			NodeControlInfo info = GetNodeControlInfoAt(point);
			return info.Node;
		}

		public NodeControlInfo GetNodeControlInfoAt(Point point)
		{
			if (point.X < 0 || point.Y < 0)
				return NodeControlInfo.Empty;

			int row = _rowLayout.GetRowAt(point);
            return row < RowCount && row >= 0 ? GetNodeControlInfoAt(RowMap[row], point) : NodeControlInfo.Empty;
        }

		private NodeControlInfo GetNodeControlInfoAt(TreeNodeAdv node, Point point)
		{
			Rectangle rect = _rowLayout.GetRowBounds(FirstVisibleRow);
			point.Y += rect.Y - ColumnHeaderHeight;
			point.X += OffsetX;
			foreach (NodeControlInfo info in GetNodeControls(node))
				if (info.Bounds.Contains(point))
					return info;

			return FullRowSelect ? new NodeControlInfo(null, Rectangle.Empty, node) : NodeControlInfo.Empty;
		}

		public void BeginUpdate()
		{
			_suspendUpdate = true;
			SuspendSelectionEvent = true;
		}

		public void EndUpdate()
		{
			_suspendUpdate = false;
			if (_needFullUpdate)
				FullUpdate();
			else
				UpdateView();
			SuspendSelectionEvent = false;
		}

		public void ExpandAll() => Root.ExpandAll();
        public void CollapseAll() => Root.CollapseAll();

        /// <summary>
		/// Expand all parent nodes, and scroll to the specified node
		/// </summary>
		public void EnsureVisible(TreeNodeAdv node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (!IsMyNode(node))
#if DEBUG
				throw new ArgumentException();
#endif
                return;

			TreeNodeAdv parent = node.Parent;
			while (parent != Root)
			{
				parent.Expand(false);
				parent = parent.Parent;
			}
			ScrollTo(node);
		}

		/// <summary>
		/// Make node visible, scroll if needed. All parent nodes of the specified node must be expanded
		/// </summary>
		/// <param name="node"></param>
		public void ScrollTo(TreeNodeAdv node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (!IsMyNode(node))
#if DEBUG
				throw new ArgumentException();
#endif
                return;

			if (node.Row < 0)
				CreateRowMap();

			int row = -1;

			if (node.Row < FirstVisibleRow)
				row = node.Row;
			else
			{
				int pageStart = _rowLayout.GetRowBounds(FirstVisibleRow).Top;
				int rowBottom = _rowLayout.GetRowBounds(node.Row).Bottom;
				if (rowBottom > pageStart + DisplayRectangle.Height - ColumnHeaderHeight)
					row = _rowLayout.GetFirstRow(node.Row);
			}

			if (row >= _vScrollBar.Minimum && row <= _vScrollBar.Maximum)
				_vScrollBar.Value = row;
		}

		public void ClearSelection()
		{
			BeginUpdate();
			try
			{
				ClearSelectionInternal();
			}
			finally
			{
				EndUpdate();
			}
		}

		internal void ClearSelectionInternal()
		{
			while (Selection.Count > 0)
			{
				var t = Selection[0];
				t.IsSelected = false;
				Selection.Remove(t); //hack
			}
		}

		#endregion

		protected override void OnSizeChanged(EventArgs e)
		{
			ArrangeControls();
			SafeUpdateScrollBars();
			base.OnSizeChanged(e);
		}

		private void ArrangeControls()
		{
			int hBarSize = _hScrollBar.Height;
			int vBarSize = _vScrollBar.Width;
			Rectangle clientRect = ClientRectangle;

			_hScrollBar.SetBounds(clientRect.X, clientRect.Bottom - hBarSize,
				clientRect.Width - vBarSize, hBarSize);

			_vScrollBar.SetBounds(clientRect.Right - vBarSize, clientRect.Y,
				vBarSize, clientRect.Height - hBarSize);
		}

		private void SafeUpdateScrollBars()
		{
			if (InvokeRequired)
				BeginInvoke(new MethodInvoker(UpdateScrollBars));
			else
				UpdateScrollBars();
		}

		private void UpdateScrollBars()
		{
			UpdateVScrollBar();
			UpdateHScrollBar();
			UpdateVScrollBar();
			UpdateHScrollBar();
			_hScrollBar.Width = DisplayRectangle.Width;
			_vScrollBar.Height = DisplayRectangle.Height;
		}

		private void UpdateHScrollBar()
		{
			_hScrollBar.Maximum = ContentWidth;
			_hScrollBar.LargeChange = Math.Max(DisplayRectangle.Width, 0);
			_hScrollBar.SmallChange = 5;
			_hScrollBar.Visible = _hScrollBar.LargeChange < _hScrollBar.Maximum;
			_hScrollBar.Value = Math.Min(_hScrollBar.Value, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
		}

		private void UpdateVScrollBar()
		{
			_vScrollBar.Maximum = Math.Max(RowCount - 1, 0);
			_vScrollBar.LargeChange = _rowLayout.PageRowCount;
			_vScrollBar.Visible = RowCount > 0 && _vScrollBar.LargeChange <= _vScrollBar.Maximum;
			_vScrollBar.Value = Math.Min(_vScrollBar.Value, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
		}

		protected override CreateParams CreateParams
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				CreateParams res = base.CreateParams;
				switch (BorderStyle)
				{
					case BorderStyle.FixedSingle:
						res.Style |= 0x800000;
						break;
					case BorderStyle.Fixed3D:
						res.ExStyle |= 0x200;
						break;
				}
				return res;
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			UpdateView();
			ChangeInput();
			base.OnGotFocus(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			_measureContext.Font = Font;
			FullUpdate();
		}

		internal IEnumerable<NodeControlInfo> GetNodeControls(TreeNodeAdv node)
		{
			if (node == null)
				yield break;
			Rectangle rowRect = _rowLayout.GetRowBounds(node.Row);
			foreach (NodeControlInfo n in GetNodeControls(node, rowRect))
				yield return n;
		}

		internal IEnumerable<NodeControlInfo> GetNodeControls(TreeNodeAdv node, Rectangle rowRect)
		{
			if (node == null)
				yield break;

			int scaledIndent = node.Tree.GetScaledSize(_indent, false);
			int y = rowRect.Y;
			int x = (node.Level - 1) * scaledIndent + LeftMargin;
			int width;
			if (node.Row == 0 && ShiftFirstNode)
				x -= scaledIndent;
			Rectangle rect;

			if (ShowPlusMinus)
			{
				width = _plusMinus.GetActualSize(node, _measureContext).Width;
				rect = new Rectangle(x, y, width, rowRect.Height);
				if (UseColumns && Columns.Count > 0 && Columns[0].Width < rect.Right)
					rect.Width = Columns[0].Width - x;

				yield return new NodeControlInfo(_plusMinus, rect, node);
				x += width;
			}

			if (!UseColumns)
			{
				foreach (NodeControl c in NodeControls)
				{
					Size s = c.GetActualSize(node, _measureContext);
					if (!s.IsEmpty)
					{
						width = s.Width;
						rect = new Rectangle(x, y, width, rowRect.Height);
						x += rect.Width;
						yield return new NodeControlInfo(c, rect, node);
					}
				}
			}
			else
			{
				int right = 0;
				foreach (TreeColumn col in Columns)
				{
					if (col.IsVisible && col.Width > 0)
					{
						right += col.Width;
						for (int i = 0; i < NodeControls.Count; i++)
						{
							NodeControl nc = NodeControls[i];
							if (nc.ParentColumn == col)
							{
								Size s = nc.GetActualSize(node, _measureContext);
								if (!s.IsEmpty)
								{
									bool isLastControl = true;
									for (int k = i + 1; k < NodeControls.Count; k++)
										if (NodeControls[k].ParentColumn == col)
										{
											isLastControl = false;
											break;
										}

									width = right - x;
									if (!isLastControl)
										width = s.Width;
									int maxWidth = Math.Max(0, right - x);
									rect = new Rectangle(x, y, Math.Min(maxWidth, width), rowRect.Height);
									x += width;
									yield return new NodeControlInfo(nc, rect, node);
								}
							}
						}
						x = right;
					}
				}
			}
		}

		internal static double Dist(Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
		}

		public void FullUpdate()
		{
			HideEditor();
			if (InvokeRequired)
				BeginInvoke(new MethodInvoker(UnsafeFullUpdate));
			else
				UnsafeFullUpdate();
		}

		private void UnsafeFullUpdate()
		{
			_rowLayout.ClearCache();
			CreateRowMap();
			SafeUpdateScrollBars();
			UpdateView();
			_needFullUpdate = false;
		}

		internal void UpdateView()
		{
			if (!_suspendUpdate)
				Invalidate(false);
		}

		internal void UpdateHeaders() => Invalidate(new Rectangle(0, 0, Width, ColumnHeaderHeight));
        internal void UpdateColumns() => FullUpdate();

        private void CreateNodes()
		{
			Selection.Clear();
			SelectionStart = null;
			Root = new TreeNodeAdv(this, null);
			Root.Expand(false);
			if (Root.Nodes.Count > 0)
				CurrentNode = Root.Nodes[0];
			else
				CurrentNode = null;
		}

		internal void ReadChilds(TreeNodeAdv parentNode) => ReadChilds(parentNode, false);

        internal void ReadChilds(TreeNodeAdv parentNode, bool performFullUpdate)
		{
			if (!parentNode.IsLeaf)
			{
				parentNode.IsExpandedOnce = true;
				parentNode.Nodes.Clear();

                IEnumerable items = Model?.GetChildren(GetPath(parentNode));
                if (items != null)
                    foreach (object obj in items)
                    {
                        AddNewNode(parentNode, obj, -1);
                        if (performFullUpdate)
                            FullUpdate();
                    }

                if (parentNode.AutoExpandOnStructureChanged)
					parentNode.ExpandAll();
			}
		}

		private void AddNewNode(TreeNodeAdv parent, object tag, int index)
		{
			TreeNodeAdv node = new TreeNodeAdv(this, tag);
			AddNode(parent, index, node);
		}

		private void AddNode(TreeNodeAdv parent, int index, TreeNodeAdv node)
		{
			if (index >= 0 && index < parent.Nodes.Count)
				parent.Nodes.Insert(index, node);
			else
				parent.Nodes.Add(node);

			node.IsLeaf = Model.IsLeaf(GetPath(node));
			if (node.IsLeaf)
				node.Nodes.Clear();
			if (!LoadOnDemand || node.IsExpandedOnce)
				ReadChilds(node);
		}

		private struct ExpandArgs
		{
			public TreeNodeAdv Node;
			public bool Value;
			public bool IgnoreChildren;
			public bool ChangedByUser;
		}

		public void AbortBackgroundExpandingThreads()
		{
			_threadPool.CancelAll(true);
			for (int i = 0; i < _expandingNodes.Count; i++)
				_expandingNodes[i].IsExpandingNow = false;
			_expandingNodes.Clear();
			Invalidate();
		}

		internal void SetIsExpanded(TreeNodeAdv node, bool value, bool ignoreChildren, bool byUser)
		{
			ExpandArgs eargs = new ExpandArgs();
			eargs.Node = node;
			eargs.Value = value;
			eargs.IgnoreChildren = ignoreChildren;
			eargs.ChangedByUser = byUser;

			if (AsyncExpanding && LoadOnDemand && !_threadPool.IsMyThread(Thread.CurrentThread))
			{
				WaitCallback wc = delegate(object argument) { SetIsExpanded((ExpandArgs)argument); };
				_threadPool.QueueUserWorkItem(wc, eargs);
			}
			else
				SetIsExpanded(eargs);
		}

		private void SetIsExpanded(ExpandArgs eargs)
		{
			bool update = !eargs.IgnoreChildren && !AsyncExpanding;
			if (update)
				BeginUpdate();
			try
			{
				if (IsMyNode(eargs.Node) && eargs.Node.IsExpanded != eargs.Value)
					SetIsExpanded(eargs.Node, eargs.Value, eargs.ChangedByUser);

				if (!eargs.IgnoreChildren)
					SetIsExpandedRecursive(eargs.Node, eargs.Value);
			}
			finally
			{
				if (update)
					EndUpdate();
			}
		}

		internal void SetIsExpanded(TreeNodeAdv node, bool value, bool byUser)
		{
			if (Root == node && !value)
				return; //Can't collapse root node

			if (value)
			{
				OnExpanding(node);
				node.OnExpanding();
			}
			else
			{
				OnCollapsing(node);
				node.OnCollapsing();
			}

			if (value && !node.IsExpandedOnce)
			{
				if (AsyncExpanding && LoadOnDemand)
				{
					AddExpandingNode(node);
					node.AssignIsExpanded(true);
					Invalidate();
				}
				ReadChilds(node, AsyncExpanding);
				RemoveExpandingNode(node);
			}
			node.AssignIsExpanded(value);
			SmartFullUpdate();

			if (value)
			{
				OnExpanded(node, byUser);
				node.OnExpanded(byUser);
			}
			else
			{
				OnCollapsed(node, byUser);
				node.OnCollapsed(byUser);
			}
		}

		private void RemoveExpandingNode(TreeNodeAdv node)
		{
			node.IsExpandingNow = false;
			_expandingNodes.Remove(node);
			if (_expandingNodes.Count <= 0)
				ExpandingIcon.Stop();
		}

		private void AddExpandingNode(TreeNodeAdv node)
		{
			node.IsExpandingNow = true;
			_expandingNodes.Add(node);
			ExpandingIcon.Start();
		}

		internal void SetIsExpandedRecursive(TreeNodeAdv root, bool value)
		{
			for (int i = 0; i < root.Nodes.Count; i++)
			{
				TreeNodeAdv node = root.Nodes[i];
                if (value)
                    node.Expand(false);
                else
                    node.Collapse(false);
				SetIsExpandedRecursive(node, value);
			}
		}

		private void CreateRowMap()
		{
			RowMap.Clear();
			int row = 0;
			ContentWidth = 0;
			foreach (TreeNodeAdv node in VisibleNodes)
			{
				node.Row = row;
				RowMap.Add(node);
				if (!UseColumns)
				{
					ContentWidth = Math.Max(ContentWidth, GetNodeWidth(node));
				}
				row++;
			}
			if (UseColumns)
			{
				ContentWidth = 0;
				foreach (TreeColumn col in _columns)
					if (col.IsVisible)
						ContentWidth += col.Width;
			}
		}

		private int GetNodeWidth(TreeNodeAdv node)
		{
			if (node.RightBounds == null)
			{
				Rectangle res = GetNodeBounds(GetNodeControls(node, Rectangle.Empty));
				node.RightBounds = res.Right;
			}
			return node.RightBounds.Value;
		}

		internal Rectangle GetNodeBounds(TreeNodeAdv node) => GetNodeBounds(GetNodeControls(node));

        private Rectangle GetNodeBounds(IEnumerable<NodeControlInfo> nodeControls)
		{
			Rectangle res = Rectangle.Empty;
			foreach (NodeControlInfo info in nodeControls)
            {
                res = res == Rectangle.Empty ? info.Bounds : Rectangle.Union(res, info.Bounds);
            }
			return res;
		}

		private void _vScrollBar_ValueChanged(object sender, EventArgs e) => FirstVisibleRow = _vScrollBar.Value;
        private void _hScrollBar_ValueChanged(object sender, EventArgs e) => OffsetX = _hScrollBar.Value;
        private void _vScrollBar_Scroll(object sender, ScrollEventArgs e) => OnScroll(e);
        private void _hScrollBar_Scroll(object sender, ScrollEventArgs e) => OnScroll(e);

        internal void SmartFullUpdate()
		{
			if (_suspendUpdate)
				_needFullUpdate = true;
			else
				FullUpdate();
		}

		internal bool IsMyNode(TreeNodeAdv node)
		{
			if (node == null)
				return false;

			if (node.Tree != this)
				return false;

			while (node.Parent != null)
				node = node.Parent;

			return node == Root;
		}

		internal void UpdateSelection()
		{
			bool flag = false;

			if (!IsMyNode(CurrentNode))
				CurrentNode = null;
			if (!IsMyNode(SelectionStart))
				SelectionStart = null;

			for (int i = Selection.Count - 1; i >= 0; i--)
				if (!IsMyNode(Selection[i]))
				{
					flag = true;
					Selection.RemoveAt(i);
				}

			if (flag)
				OnSelectionChanged();
		}

		internal void ChangeColumnWidth(TreeColumn column)
		{
			if (!(Input is ResizeColumnState))
			{
				FullUpdate();
				OnColumnWidthChanged(column);
			}
		}

		public TreeNodeAdv FindNode(TreePath path)
		{
			return FindNode(path, false);
		}

		public TreeNodeAdv FindNode(TreePath path, bool readChilds) => path.IsEmpty() ? Root : FindNode(Root, path, 0, readChilds);

        private TreeNodeAdv FindNode(TreeNodeAdv root, TreePath path, int level, bool readChilds)
		{
			if (!root.IsExpandedOnce && readChilds)
				ReadChilds(root);

			for (int i = 0; i < root.Nodes.Count; i++)
			{
				TreeNodeAdv node = root.Nodes[i];
				if (node.Tag == path.FullPath[level])
                {
                    return level == path.FullPath.Length - 1
                        ? node
                        : FindNode(node, path, level + 1, readChilds);
                }
			}
			return null;
		}

		public TreeNodeAdv FindNodeByTag(object tag) => FindNodeByTag(Root, tag);

        private TreeNodeAdv FindNodeByTag(TreeNodeAdv root, object tag)
		{
			foreach (TreeNodeAdv node in root.Nodes)
			{
				if (node.Tag == tag)
					return node;
				TreeNodeAdv res = FindNodeByTag(node, tag);
				if (res != null)
					return res;
			}
			return null;
		}

		public void SelectAllNodes()
		{
			SuspendSelectionEvent = true;
			try
			{
				if (SelectionMode == TreeSelectionMode.MultiSameParent)
				{
					if (CurrentNode != null)
					{
						foreach (TreeNodeAdv n in CurrentNode.Parent.Nodes)
							n.IsSelected = true;
					}
				}
				else if (SelectionMode == TreeSelectionMode.Multi)
				{
					SelectNodes(Root.Nodes);
				}
			}
			finally
			{
				SuspendSelectionEvent = false;
			}
		}

		private void SelectNodes(Collection<TreeNodeAdv> nodes)
		{
			foreach (TreeNodeAdv n in nodes)
			{
				n.IsSelected = true;
				if (n.IsExpanded)
					SelectNodes(n.Nodes);
			}
		}

		#region ModelEvents
		private void BindModelEvents()
		{
			_model.NodesChanged += _model_NodesChanged;
			_model.NodesInserted += _model_NodesInserted;
			_model.NodesRemoved += _model_NodesRemoved;
			_model.StructureChanged += _model_StructureChanged;
		}

		private void UnbindModelEvents()
		{
			_model.NodesChanged -= _model_NodesChanged;
			_model.NodesInserted -= _model_NodesInserted;
			_model.NodesRemoved -= _model_NodesRemoved;
			_model.StructureChanged -= _model_StructureChanged;
		}

		private void _model_StructureChanged(object sender, TreePathEventArgs e)
		{
			if (e.Path == null)
				throw new ArgumentNullException();

			TreeNodeAdv node = FindNode(e.Path);
            if (node == null)
                return;

            if (node != Root)
                node.IsLeaf = Model.IsLeaf(GetPath(node));

            var list = new Dictionary<object, object>();
            SaveExpandedNodes(node, list);
            ReadChilds(node);
            RestoreExpandedNodes(node, list);

            UpdateSelection();
            SmartFullUpdate();
        }

		private void RestoreExpandedNodes(TreeNodeAdv node, Dictionary<object, object> list)
		{
			if (node.Tag != null && list.ContainsKey(node.Tag))
			{
				node.Expand(false);
				foreach (var child in node.Children)
					RestoreExpandedNodes(child, list);
			}
		}

		private void SaveExpandedNodes(TreeNodeAdv node, Dictionary<object, object> list)
		{
			if (node.IsExpanded && node.Tag != null)
			{
				list.Add(node.Tag, null);
				foreach (var child in node.Children)
					SaveExpandedNodes(child, list);
			}
		}

		private void _model_NodesRemoved(object sender, TreeModelEventArgs e)
		{
			TreeNodeAdv parent = FindNode(e.Path);
			if (parent != null)
			{
				if (e.Indices != null)
				{
					List<int> list = new List<int>(e.Indices);
					list.Sort();
					for (int n = list.Count - 1; n >= 0; n--)
					{
						int index = list[n];
						if (index >= 0 && index <= parent.Nodes.Count)
							parent.Nodes.RemoveAt(index);
					}
				}
				else
				{
					for (int i = parent.Nodes.Count - 1; i >= 0; i--)
					{
						for (int n = 0; n < e.Children.Length; n++)
							if (parent.Nodes[i].Tag == e.Children[n])
							{
								parent.Nodes.RemoveAt(i);
								break;
							}
					}
				}
			}
			UpdateSelection();
			SmartFullUpdate();
		}

		private void _model_NodesInserted(object sender, TreeModelEventArgs e)
		{
			if (e.Indices == null)
				throw new ArgumentNullException(nameof(e.Indices));

			TreeNodeAdv parent = FindNode(e.Path);
			if (parent != null)
			{
				for (int i = 0; i < e.Children.Length; i++)
					AddNewNode(parent, e.Children[i], e.Indices[i]);
			}
			SmartFullUpdate();
		}

		private void _model_NodesChanged(object sender, TreeModelEventArgs e)
		{
			TreeNodeAdv parent = FindNode(e.Path);
			if (parent != null && parent.IsVisible && parent.IsExpanded)
			{
				if (InvokeRequired)
					BeginInvoke(new UpdateContentWidthDelegate(ClearNodesSize), e, parent);
				else
					ClearNodesSize(e, parent);
				SmartFullUpdate();
			}
		}

		private delegate void UpdateContentWidthDelegate(TreeModelEventArgs e, TreeNodeAdv parent);
		private static void ClearNodesSize(TreeModelEventArgs e, TreeNodeAdv parent)
		{
			if (e.Indices != null)
			{
				foreach (int index in e.Indices)
				{
					if (index >= 0 && index < parent.Nodes.Count)
					{
						TreeNodeAdv node = parent.Nodes[index];
						node.Height = node.RightBounds = null;
					}
				}
			}
			else
			{
				foreach (TreeNodeAdv node in parent.Nodes)
				{
					foreach (object obj in e.Children)
						if (node.Tag == obj)
						{
							node.Height = node.RightBounds = null;
						}
				}
			}
		}
		#endregion
	}
}

using System;
using System.ComponentModel;

namespace Aga.Controls.Tree.NodeControls
{
	public abstract class InteractiveControl : BindableControl
	{
		private bool _editEnabled;
		[DefaultValue(false)]
		public bool EditEnabled
		{
			get { return _editEnabled; }
			set { _editEnabled = value; }
		}

		protected bool IsEditEnabled(TreeNodeAdv node)
        {
            if (EditEnabled)
			{
				NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
				args.Value = true;
				OnIsEditEnabledValueNeeded(args);
				return Convert.ToBoolean(args.Value);
			}

            return false;
        }

		public event EventHandler<NodeControlValueEventArgs> IsEditEnabledValueNeeded;
		private void OnIsEditEnabledValueNeeded(NodeControlValueEventArgs args)
        {
            IsEditEnabledValueNeeded?.Invoke(this, args);
        }
	}
}

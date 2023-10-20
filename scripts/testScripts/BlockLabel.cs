using System;
using System.Globalization;
using Godot;
using TerrariaRipoffNNF.scripts.BlockScripts;

namespace TerrariaRipoffNNF.scripts.testScripts; 

public partial class BlockLabel : Label {
	private BlockNode parent;

	public override void _Ready() {
		parent = GetOwner<BlockNode>();
		UpdateText();
		parent.Block.Health.OnHealthChanged += Block_OnHealthChanged;
	}

	private void UpdateText() {
		Text = parent.Block.Health.CurrentHealth.ToString(CultureInfo.InvariantCulture);
	}
	
	private void Block_OnHealthChanged(object sender, EventArgs e) {
		UpdateText();
	}
}
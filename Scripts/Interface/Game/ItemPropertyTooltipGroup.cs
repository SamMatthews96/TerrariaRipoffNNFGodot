using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class ItemPropertyTooltipGroup : Container {
    [Export] private Container _propertyLabelContainer;
    [Export] private Label _titleLabel;

    public static ItemPropertyTooltipGroup Create(
        string propertyName, Dictionary itemProperty) {
        ItemPropertyTooltipGroup newGroup =
            Manager.Instance.PackedScenes.ItemTooltipPropertyGroup
                .Instantiate<ItemPropertyTooltipGroup>();

        foreach (Node node in newGroup._propertyLabelContainer.GetChildren()) {
            node.QueueFree();
        }

        newGroup._titleLabel.Text = propertyName;
        foreach ((Variant key, Variant value) in itemProperty) {
            Label newLabel = new();
            newLabel.Text = $"{key.ToString()}: {value.ToString()}";
            newGroup._propertyLabelContainer.AddChild(newLabel);
        }

        return newGroup;
    }
}
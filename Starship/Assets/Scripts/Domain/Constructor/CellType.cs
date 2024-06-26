using GameDatabase.Enums;

namespace Constructor
{
	public static class ComponentTypeExtension
	{
		public static bool CompatibleWith(this CellType component, CellType target)
		{
			if (target == CellType.Empty || target == CellType.Custom)
				return false;
			if (component == CellType.Empty || component == target)
				return true;
			if (target == CellType.InnerOuter && (component == CellType.Inner || component == CellType.Outer))
				return true;
            if (component == CellType.InnerOuter && (target == CellType.Inner || target == CellType.Outer))
				return true;
			if (target == CellType.Core2Block && (component == CellType.CoreBlock || component == CellType.Inner))
				return true;
            if (component == CellType.Core2Block && (target == CellType.CoreBlock || target == CellType.Inner))
                return true;
			if (target == CellType.StructureBlock && (component == CellType.StructureBlock || component == CellType.Outer))
				return true;
			if (component == CellType.StructureBlock && (target == CellType.StructureBlock || component == CellType.Outer))
				return true;
			return false;
		}
	}
}

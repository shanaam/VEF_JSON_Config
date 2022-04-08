using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ColourPaletteHelper
{
    /// <summary>
    /// Given a Color,
    /// this method sets the normal, selected, disabled, pressed, highlighted colours of a colour block. 
    /// </summary>
    /// <param name="colour">The main colour of the selectable to base the palette off of.</param>
    /// <param name="colourMultiplier">The intensity of the colour</param>
    public static ColorBlock SetColourPalette(Color colour, float colourMultiplier)
    {
        Color fullAlpha = new Color(0, 0, 0, 1);
        ColorBlock colourBlock = new ColorBlock
        {
            normalColor = colour,
            selectedColor = colour,
            disabledColor = colour * .18f + fullAlpha,
            pressedColor = colour * .18f + fullAlpha,
            highlightedColor = colour * .25f + fullAlpha,
            colorMultiplier = colourMultiplier
        };

        return colourBlock;
    }
}

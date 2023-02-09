using System;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines marker visual styles.
    /// </summary>
    /// <remarks>Equivalent to <see cref="MARKERVISUAL"/> and <see cref="MARKERVISUAL2"/>.</remarks>
    [Flags]
    public enum MarkerVisualStyle
    {
        /// <summary>
        /// No style.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates that a box is drawn around the marked text.
        /// </summary>
        Border = MARKERVISUAL.MV_BORDER,
        /// <summary>
        /// Indicates that the marked text should always be colored inline.
        /// </summary>
        ColorAlways = MARKERVISUAL.MV_COLOR_ALWAYS,
        /// <summary>
        /// Indicates that the marked text should be colored only if the widget margin is hidden.
        /// </summary>
        ColorLineIfNoMargin = MARKERVISUAL.MV_COLOR_LINE_IF_NO_MARGIN,
        /// <summary>
        /// Indicates that a marker should paint as a solid bar if the text span is of zero length.
        /// </summary>
        ColorSpanIfZeroLength = MARKERVISUAL.MV_COLOR_SPAN_IF_ZERO_LENGTH,
        /// <summary>
        /// Indicates that the body of a marker wants to contribute context.
        /// </summary>
        ContextContributionForBody = MARKERVISUAL.MV_CONTEXT_CONTRIBUTION_FOR_BODY,
        /// <summary>
        /// Indicates that a glyph can take part in drag and drop operations.
        /// </summary>
        DraggableGlyph = MARKERVISUAL.MV_DRAGGABLE_GLYPH,
        /// <summary>
        /// Forces the marker to be invisible.
        /// </summary>
        ForceInvisible = MARKERVISUAL.MV_FORCE_INVISIBLE,
        /// <summary>
        /// Can show a glyph in the widget margin.
        /// </summary>
        Glyph = MARKERVISUAL.MV_GLYPH,
        /// <summary>
        /// Indicates that the client has requested a callback to set the mouse cursor when the user hovers the mouse over the glyph.
        /// </summary>
        GlyphHoverCursor = MARKERVISUAL.MV_GLYPH_HOVER_CURSOR,
        /// <summary>
        /// Marker is only a line adornment and does not otherwise affect coloring.
        /// </summary>
        Line = MARKERVISUAL.MV_LINE,
        /// <summary>
        /// Indicates that a glyph spans multiple lines.
        /// </summary>
        MultilineGlyph = MARKERVISUAL.MV_MULTILINE_GLYPH,
        /// <summary>
        /// Indicates that the glyph lives in the selection margin, not the normal widget margin.
        /// </summary>
        SelectionMarginGlyph = MARKERVISUAL.MV_SEL_MARGIN_GLYPH,
        /// <summary>
        /// Determines whether a tip should be shown for the body of the marker text.
        /// </summary>
        TipForBody = MARKERVISUAL.MV_TIP_FOR_BODY,
        /// <summary>
        /// Determines whether a tip should be shown in the widget margin.
        /// </summary>
        TipForGlyph = MARKERVISUAL.MV_TIP_FOR_GLYPH,
        /// <summary>
        /// Draw foreground text in bold.
        /// </summary>
        BoldText = MARKERVISUAL2.MV_BOLDTEXT,
        /// <summary>
        /// Indicates that the background color is not customizable.
        /// </summary>
        DisallowBacgroundChange = MARKERVISUAL2.MV_DISALLOWBGCHANGE,
        /// <summary>
        /// Indicates that the foreground color is not customizable.
        /// </summary>
        DisallowForegroundChange = MARKERVISUAL2.MV_DISALLOWFGCHANGE,
        /// <summary>
        /// Forces a <see cref="MARKERBEHAVIORFLAGS.MB_MULTILINESPAN"/> or <see cref="MARKERBEHAVIORFLAGS.MB_LINESPAN"/> marker to paint to the closest viewable location on the line.
        /// </summary>
        ForceClosestIfHidden = MARKERVISUAL2.MV_FORCE_CLOSEST_IF_HIDDEN,
        /// <summary>
        /// Draw a rounded border.
        /// </summary>
        RoundedBorder = MARKERVISUAL2.MV_ROUNDEDBORDER,
        /// <summary>
        /// Forces a <see cref="MARKERBEHAVIORFLAGS.MB_MULTILINESPAN"/> or <see cref="MARKERBEHAVIORFLAGS.MB_LINESPAN"/> marker to paint a full line even if part of the marker is hidden.
        /// </summary>
        SelectWholeLine = MARKERVISUAL2.MV_SELECT_WHOLE_LINE,
        /// <summary>
        /// Marker for smart tags.
        /// </summary>
        SmartTag = MARKERVISUAL2.MV_SMARTTAG,
        /// <summary>
        /// Marker for change tracking.
        /// </summary>
        Track = MARKERVISUAL2.MV_TRACK
    }
}

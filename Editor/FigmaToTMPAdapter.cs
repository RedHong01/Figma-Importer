using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace FigmaImporter.Editor
{
    [Serializable]
    internal sealed class FigmaTextStyle
    {
        // Figma typography inputs (px unless noted).
        public float fontSizePx;
        public float lineHeightPx;
        public float lineHeightPxRaw;
        public float lineHeightPercent;
        public float lineHeightPercentFontSize;
        public string lineHeightUnit;
        public float letterSpacingPx;
        public float paragraphIndentPx;
        public float fixedWidthPx;
        public float fixedHeightPx;
        public bool hasRenderBounds;
        public float renderInsetLeftPx;
        public float renderInsetTopPx;
        public float renderInsetRightPx;
        public float renderInsetBottomPx;
        public float renderWidthPx;
        public float renderHeightPx;
        public string textAlignHorizontal;
        public string textAlignVertical;
        public string textAutoResize;
        public string textTruncation;
        public string leadingTrim;
        public int maxLines;

        public static FigmaTextStyle FromNode(Node node, float scale)
        {
            var style = node?.style;
            var safeScale = Mathf.Max(Mathf.Abs(scale), 0.0001f);

            var fontSizePx = Mathf.Max(GetOptionalStyleFloat(style, "fontSize", 14f) * safeScale, 0.5f);
            var lineHeightPxRaw = Mathf.Max(GetOptionalStyleFloat(style, "lineHeightPx") * safeScale, 0f);
            var lineHeightPercent = Mathf.Max(GetOptionalStyleFloat(style, "lineHeightPercent"), 0f);
            var lineHeightPercentFontSize = Mathf.Max(GetOptionalStyleFloat(style, "lineHeightPercentFontSize"), 0f);

            var fixedWidthSource = node?.absoluteBoundingBox != null
                ? node.absoluteBoundingBox.width
                : 0f;
            var fixedHeightSource = node?.absoluteBoundingBox != null
                ? node.absoluteBoundingBox.height
                : 0f;
            var hasRenderBounds = node?.absoluteBoundingBox != null && node?.absoluteRenderBounds != null;
            var renderInsetLeftPx = 0f;
            var renderInsetTopPx = 0f;
            var renderInsetRightPx = 0f;
            var renderInsetBottomPx = 0f;
            var renderWidthPx = 0f;
            var renderHeightPx = 0f;

            if (hasRenderBounds)
            {
                var bounding = node.absoluteBoundingBox;
                var render = node.absoluteRenderBounds;
                renderInsetLeftPx = (render.x - bounding.x) * safeScale;
                renderInsetTopPx = (render.y - bounding.y) * safeScale;
                renderInsetRightPx = ((bounding.x + bounding.width) - (render.x + render.width)) * safeScale;
                renderInsetBottomPx = ((bounding.y + bounding.height) - (render.y + render.height)) * safeScale;
                renderWidthPx = Mathf.Max(render.width * safeScale, 0f);
                renderHeightPx = Mathf.Max(render.height * safeScale, 0f);
            }

            return new FigmaTextStyle
            {
                fontSizePx = fontSizePx,
                lineHeightPxRaw = lineHeightPxRaw,
                lineHeightPercent = lineHeightPercent,
                lineHeightPercentFontSize = lineHeightPercentFontSize,
                lineHeightUnit = GetOptionalStyleString(style, "lineHeightUnit", string.Empty),
                lineHeightPx = ResolveFallbackLineHeightPx(fontSizePx, lineHeightPxRaw, lineHeightPercentFontSize, lineHeightPercent),
                letterSpacingPx = GetOptionalStyleFloat(style, "letterSpacing") * safeScale,
                paragraphIndentPx = GetOptionalStyleFloat(style, "paragraphIndent") * safeScale,
                fixedWidthPx = Mathf.Max(fixedWidthSource * safeScale, 0f),
                fixedHeightPx = Mathf.Max(fixedHeightSource * safeScale, 0f),
                hasRenderBounds = hasRenderBounds,
                renderInsetLeftPx = renderInsetLeftPx,
                renderInsetTopPx = renderInsetTopPx,
                renderInsetRightPx = renderInsetRightPx,
                renderInsetBottomPx = renderInsetBottomPx,
                renderWidthPx = renderWidthPx,
                renderHeightPx = renderHeightPx,
                textAlignHorizontal = GetOptionalStyleString(style, "textAlignHorizontal", "LEFT"),
                textAlignVertical = GetOptionalStyleString(style, "textAlignVertical", "TOP"),
                textAutoResize = GetOptionalStyleString(style, "textAutoResize", string.Empty),
                textTruncation = node?.textTruncation ?? string.Empty,
                leadingTrim = GetOptionalStyleString(style, "leadingTrim", string.Empty),
                maxLines = node?.maxLines ?? 0
            };
        }

        private static float ResolveFallbackLineHeightPx(
            float fontSizePx,
            float lineHeightPxRaw,
            float lineHeightPercentFontSize,
            float lineHeightPercent)
        {
            if (lineHeightPxRaw > 0f)
            {
                return lineHeightPxRaw;
            }

            if (lineHeightPercentFontSize > 0f)
            {
                return fontSizePx * (lineHeightPercentFontSize / 100f);
            }

            if (lineHeightPercent > 0f)
            {
                return fontSizePx * (lineHeightPercent / 100f);
            }

            return fontSizePx;
        }

        private static float GetOptionalStyleFloat(Style style, string name, float fallback = 0f)
        {
            if (style == null || string.IsNullOrWhiteSpace(name))
            {
                return fallback;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                var type = style.GetType();
                var field = type.GetField(name, Flags);
                if (field != null)
                {
                    var value = field.GetValue(style);
                    if (value is float floatValue)
                    {
                        return floatValue;
                    }

                    if (value is double doubleValue)
                    {
                        return (float)doubleValue;
                    }

                    if (value is int intValue)
                    {
                        return intValue;
                    }
                }

                var property = type.GetProperty(name, Flags);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(style);
                    if (value is float floatValue)
                    {
                        return floatValue;
                    }

                    if (value is double doubleValue)
                    {
                        return (float)doubleValue;
                    }

                    if (value is int intValue)
                    {
                        return intValue;
                    }
                }
            }
            catch
            {
            }

            return fallback;
        }

        private static string GetOptionalStyleString(Style style, string name, string fallback = "")
        {
            if (style == null || string.IsNullOrWhiteSpace(name))
            {
                return fallback;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                var type = style.GetType();
                var field = type.GetField(name, Flags);
                if (field != null)
                {
                    var value = field.GetValue(style) as string;
                    return value ?? fallback;
                }

                var property = type.GetProperty(name, Flags);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(style) as string;
                    return value ?? fallback;
                }
            }
            catch
            {
            }

            return fallback;
        }
    }

    internal sealed class FigmaToTMPAdapter
    {
        private const string ScaleProbeText = "MMMMMMMMMM";
        private const float MinFontSizePx = 0.5f;
        private const float MinWidthForScaleFix = 4f;
        private const float MaxSpacingAdjustmentStep = 12f;
        private const float MaxLineSpacingAdjustmentStep = 12f;

        private static readonly Dictionary<string, float> ScaleFixCache =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public bool enableScaleCorrection = true;
        public bool enableDebugLog;
        public bool escapeInputText = true;
        public bool preferMetricLineSpacing = true;

        // Main API: maps Figma typography onto TMP with a hybrid strategy:
        // - keep metric-aware line-height conversion (legacy strength)
        // - keep rich-text first-line indent + width constraint + scale fix (new strength)
        public void Apply(TMP_Text tmp, FigmaTextStyle style, string text)
        {
            if (tmp == null || style == null)
            {
                return;
            }

            var safeFontSize = Mathf.Max(style.fontSizePx, MinFontSizePx);
            tmp.richText = true;
            tmp.enableAutoSizing = false;
            tmp.margin = Vector4.zero;
            tmp.fontSize = safeFontSize;

            ApplyAlignment(tmp, style);
            ApplyWidthConstraint(tmp, style.fixedWidthPx);
            ApplyAutoResizeBehavior(tmp, style);

            var scaleFix = enableScaleCorrection
                ? GetOrComputeScaleFix(tmp, style, safeFontSize, text)
                : 1f;

            var targetLineHeight = ResolveTargetLineHeight(tmp, style, safeFontSize);
            tmp.lineSpacing = ComputeBestLineSpacing(tmp, targetLineHeight, safeFontSize, preferMetricLineSpacing);

            tmp.characterSpacing = ComputeCharacterSpacing(style.letterSpacingPx, safeFontSize);

            var finalIndent = style.paragraphIndentPx * scaleFix;
            tmp.text = BuildIndentedText(text, finalIndent, escapeInputText);
            if (!style.hasRenderBounds)
            {
                ApplyLeadingTrimCompensation(tmp, style, safeFontSize);
            }
            ApplyPostLayoutTypographyFit(tmp, style, safeFontSize, text);
            if (style.hasRenderBounds)
            {
                ApplyRenderBoundsAlignment(tmp, style);
            }

            MarkLayoutDirty(tmp);

            if (!enableDebugLog)
            {
                return;
            }

            Debug.Log(
                $"[FigmaImporter] FigmaToTMPAdapter font={safeFontSize:F3}px targetLineHeight={targetLineHeight:F3}px lineSpacing={tmp.lineSpacing:F4} " +
                $"letterSpacingPx={style.letterSpacingPx:F3} characterSpacing={tmp.characterSpacing:F3} indentPx={style.paragraphIndentPx:F3} " +
                $"scaleFix={scaleFix:F4} finalIndentPx={finalIndent:F3} widthPx={style.fixedWidthPx:F3} heightPx={style.fixedHeightPx:F3} " +
                $"renderBounds={style.hasRenderBounds} unit={style.lineHeightUnit}");
        }

        public static void ClearScaleFixCache()
        {
            ScaleFixCache.Clear();
        }

        private static float ResolveTargetLineHeight(TMP_Text tmp, FigmaTextStyle style, float safeFontSize)
        {
            if (style == null)
            {
                return safeFontSize;
            }

            var unit = (style.lineHeightUnit ?? string.Empty).Trim().ToUpperInvariant();
            if (unit == "AUTO")
            {
                return -1f;
            }

            if (unit == "PIXELS" && style.lineHeightPxRaw > 0f)
            {
                return style.lineHeightPxRaw;
            }

            if (unit == "FONT_SIZE_%" && style.lineHeightPercentFontSize > 0f)
            {
                return safeFontSize * (style.lineHeightPercentFontSize / 100f);
            }

            if (unit == "INTRINSIC_%" && style.lineHeightPercent > 0f)
            {
                if (TryEstimateBaseLineHeight(tmp, safeFontSize, out var baseLineHeight) && baseLineHeight > 0f)
                {
                    return baseLineHeight * (style.lineHeightPercent / 100f);
                }
            }

            if (style.lineHeightPxRaw > 0f)
            {
                return style.lineHeightPxRaw;
            }

            if (style.lineHeightPercentFontSize > 0f)
            {
                return safeFontSize * (style.lineHeightPercentFontSize / 100f);
            }

            if (style.lineHeightPercent > 0f)
            {
                if (TryEstimateBaseLineHeight(tmp, safeFontSize, out var baseLineHeight) && baseLineHeight > 0f)
                {
                    return baseLineHeight * (style.lineHeightPercent / 100f);
                }

                return safeFontSize * (style.lineHeightPercent / 100f);
            }

            return style.lineHeightPx > 0f ? style.lineHeightPx : safeFontSize;
        }

        private static float ComputeBestLineSpacing(TMP_Text tmp, float targetLineHeightPx, float fontSizePx, bool preferMetric)
        {
            if (targetLineHeightPx <= 0f)
            {
                return 0f;
            }

            if (preferMetric && TryEstimateBaseLineHeight(tmp, fontSizePx, out var baseLineHeight) && baseLineHeight > 0f)
            {
                var metricLineSpacing = targetLineHeightPx - baseLineHeight;
                if (!float.IsNaN(metricLineSpacing) && !float.IsInfinity(metricLineSpacing))
                {
                    return metricLineSpacing;
                }
            }

            var safeFontSize = Mathf.Max(fontSizePx, 0.0001f);
            var formulaLineSpacing = (targetLineHeightPx - safeFontSize) / safeFontSize;
            if (float.IsNaN(formulaLineSpacing) || float.IsInfinity(formulaLineSpacing))
            {
                return 0f;
            }

            return formulaLineSpacing;
        }

        private static bool TryEstimateBaseLineHeight(TMP_Text tmp, float fontSizePx, out float baseLineHeight)
        {
            baseLineHeight = 0f;
            if (tmp == null)
            {
                return false;
            }

            var safeFontSize = Mathf.Max(fontSizePx, MinFontSizePx);
            var font = tmp.font;
            if (font == null)
            {
                baseLineHeight = safeFontSize;
                return true;
            }

            var faceInfo = font.faceInfo;
            if (faceInfo.pointSize > 0f && faceInfo.lineHeight > 0f)
            {
                baseLineHeight = (faceInfo.lineHeight / faceInfo.pointSize) * safeFontSize;
                return true;
            }

            baseLineHeight = safeFontSize;
            return true;
        }

        private static float ComputeCharacterSpacing(float figmaLetterSpacingPx, float fontSizePx)
        {
            var safeFontSize = Mathf.Max(fontSizePx, MinFontSizePx);
            var normalizedEm = figmaLetterSpacingPx / safeFontSize;
            var spacing = normalizedEm * 100f;
            if (float.IsNaN(spacing) || float.IsInfinity(spacing))
            {
                return 0f;
            }

            return spacing;
        }

        private static void ApplyWidthConstraint(TMP_Text tmp, float fixedWidthPx)
        {
            if (tmp == null || tmp.rectTransform == null || fixedWidthPx <= 0f)
            {
                return;
            }

            tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedWidthPx);
        }

        private static void ApplyAlignment(TMP_Text tmp, FigmaTextStyle style)
        {
            if (tmp == null || style == null)
            {
                return;
            }

            var h = (style.textAlignHorizontal ?? string.Empty).Trim().ToUpperInvariant();
            var v = (style.textAlignVertical ?? string.Empty).Trim().ToUpperInvariant();

            switch (h)
            {
                case "CENTER":
                    tmp.alignment = v == "BOTTOM"
                        ? TextAlignmentOptions.Bottom
                        : v == "CENTER"
                            ? TextAlignmentOptions.Center
                            : TextAlignmentOptions.Top;
                    return;
                case "RIGHT":
                    tmp.alignment = v == "BOTTOM"
                        ? TextAlignmentOptions.BottomRight
                        : v == "CENTER"
                            ? TextAlignmentOptions.Right
                            : TextAlignmentOptions.TopRight;
                    return;
                case "JUSTIFIED":
                    tmp.alignment = v == "BOTTOM"
                        ? TextAlignmentOptions.BottomJustified
                        : v == "CENTER"
                            ? TextAlignmentOptions.Justified
                            : TextAlignmentOptions.TopJustified;
                    return;
                default:
                    tmp.alignment = v == "BOTTOM"
                        ? TextAlignmentOptions.BottomLeft
                        : v == "CENTER"
                            ? TextAlignmentOptions.Left
                            : TextAlignmentOptions.TopLeft;
                    return;
            }
        }

        private static void ApplyAutoResizeBehavior(TMP_Text tmp, FigmaTextStyle style)
        {
            if (tmp == null || style == null)
            {
                return;
            }

            var autoResize = (style.textAutoResize ?? string.Empty).Trim().ToUpperInvariant();
            var truncation = (style.textTruncation ?? string.Empty).Trim().ToUpperInvariant();
            var isTruncate = autoResize == "TRUNCATE" || truncation == "ENDING";

            switch (autoResize)
            {
                case "WIDTH_AND_HEIGHT":
                    SetWordWrapping(tmp, false);
                    break;
                case "HEIGHT":
                case "NONE":
                    SetWordWrapping(tmp, true);
                    break;
                case "TRUNCATE":
                    SetWordWrapping(tmp, style.maxLines != 1);
                    break;
            }

            tmp.overflowMode = isTruncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
            tmp.maxVisibleLines = style.maxLines > 0 ? style.maxLines : 99999;

            if (isTruncate && style.maxLines == 1)
            {
                SetWordWrapping(tmp, false);
            }
        }

        private static float GetOrComputeScaleFix(TMP_Text tmp, FigmaTextStyle style, float safeFontSize, string text)
        {
            if (tmp == null || style == null || style.fixedWidthPx <= MinWidthForScaleFix)
            {
                return 1f;
            }

            var cacheKey = BuildScaleFixCacheKey(tmp, style);
            if (ScaleFixCache.TryGetValue(cacheKey, out var cachedFix) && cachedFix > 0f)
            {
                return cachedFix;
            }

            var previousAutoSizing = tmp.enableAutoSizing;
            var previousFontSize = tmp.fontSize;
            var previousCharacterSpacing = tmp.characterSpacing;
            float unityWidth;

            try
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = safeFontSize;
                tmp.characterSpacing = 0f;
                var sampleText = string.IsNullOrWhiteSpace(text)
                    ? ScaleProbeText
                    : text;
                unityWidth = tmp.GetPreferredValues(sampleText, float.PositiveInfinity, float.PositiveInfinity).x;
            }
            finally
            {
                tmp.fontSize = previousFontSize;
                tmp.enableAutoSizing = previousAutoSizing;
                tmp.characterSpacing = previousCharacterSpacing;
            }

            if (unityWidth <= 0.0001f || float.IsNaN(unityWidth) || float.IsInfinity(unityWidth))
            {
                return 1f;
            }

            var scaleFix = style.fixedWidthPx / unityWidth;
            if (float.IsNaN(scaleFix) || float.IsInfinity(scaleFix) || scaleFix <= 0f)
            {
                scaleFix = 1f;
            }

            scaleFix = Mathf.Clamp(scaleFix, 0.1f, 10f);
            ScaleFixCache[cacheKey] = scaleFix;
            return scaleFix;
        }

        private static void ApplyLeadingTrimCompensation(TMP_Text tmp, FigmaTextStyle style, float fontSizePx)
        {
            if (tmp == null || style == null || tmp.font == null)
            {
                return;
            }

            var trimMode = (style.leadingTrim ?? string.Empty).Trim().ToUpperInvariant();
            if (!string.Equals(trimMode, "CAP_HEIGHT", StringComparison.Ordinal))
            {
                return;
            }

            var faceInfo = tmp.font.faceInfo;
            if (faceInfo.pointSize <= 0f)
            {
                return;
            }

            var ascentPx = (faceInfo.ascentLine / faceInfo.pointSize) * fontSizePx;
            var capPx = (faceInfo.capLine / faceInfo.pointSize) * fontSizePx;
            var topTrimPx = Mathf.Max(0f, ascentPx - capPx);
            if (topTrimPx <= 0.001f)
            {
                return;
            }

            // TMP top margin uses opposite sign convention for downward shift.
            var margin = tmp.margin;
            margin.y = -topTrimPx;
            tmp.margin = margin;
        }

        private static void ApplyPostLayoutTypographyFit(TMP_Text tmp, FigmaTextStyle style, float safeFontSize, string sourceText)
        {
            if (tmp == null || style == null)
            {
                return;
            }

            var normalizedText = (sourceText ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            if (normalizedText.Length == 0)
            {
                return;
            }

            var autoResize = (style.textAutoResize ?? string.Empty).Trim().ToUpperInvariant();
            var wraps = autoResize == "HEIGHT" || autoResize == "NONE" || (autoResize == "TRUNCATE" && style.maxLines != 1);

            // Width fit: adjust character spacing by measured delta (small bounded step).
            if (!wraps && style.fixedWidthPx > MinWidthForScaleFix)
            {
                var preferred = tmp.GetPreferredValues(tmp.text, float.PositiveInfinity, float.PositiveInfinity);
                if (preferred.x > 0.001f && !float.IsNaN(preferred.x) && !float.IsInfinity(preferred.x))
                {
                    var deltaWidth = style.fixedWidthPx - preferred.x;
                    var gapCount = Mathf.Max(CountVisibleCharacters(normalizedText) - 1, 1);
                    var spacingStepPx = Mathf.Clamp(deltaWidth / gapCount, -MaxSpacingAdjustmentStep, MaxSpacingAdjustmentStep);
                    var spacingStep = ComputeCharacterSpacing(spacingStepPx, safeFontSize);
                    if (!float.IsNaN(spacingStep) && !float.IsInfinity(spacingStep))
                    {
                        tmp.characterSpacing += spacingStep;
                    }
                }
            }

            // Height fit: for multi-line text, nudge line spacing by measured delta.
            if (style.fixedHeightPx > MinWidthForScaleFix)
            {
                tmp.ForceMeshUpdate();
                var lineCount = tmp.textInfo != null ? Mathf.Max(1, tmp.textInfo.lineCount) : 1;
                if (lineCount > 1)
                {
                    var preferred = tmp.GetPreferredValues(tmp.text, style.fixedWidthPx > 0f ? style.fixedWidthPx : float.PositiveInfinity, float.PositiveInfinity);
                    if (preferred.y > 0.001f && !float.IsNaN(preferred.y) && !float.IsInfinity(preferred.y))
                    {
                        var deltaHeight = style.fixedHeightPx - preferred.y;
                        var step = Mathf.Clamp(deltaHeight / (lineCount - 1), -MaxLineSpacingAdjustmentStep, MaxLineSpacingAdjustmentStep);
                        if (!float.IsNaN(step) && !float.IsInfinity(step))
                        {
                            tmp.lineSpacing += step;
                        }
                    }
                }
            }
        }

        private static void ApplyRenderBoundsAlignment(TMP_Text tmp, FigmaTextStyle style)
        {
            if (tmp == null || style == null || !style.hasRenderBounds || tmp.rectTransform == null)
            {
                return;
            }

            if (!TryMeasureRenderedInsets(tmp, out var actualInsets))
            {
                return;
            }

            var targetLeft = style.renderInsetLeftPx;
            var targetTop = style.renderInsetTopPx;
            var shiftX = targetLeft - actualInsets.x;
            var shiftY = targetTop - actualInsets.y;
            if (Mathf.Abs(shiftX) <= 0.01f && Mathf.Abs(shiftY) <= 0.01f)
            {
                return;
            }

            var maxShiftX = Mathf.Max(style.fixedWidthPx, 1f);
            var maxShiftY = Mathf.Max(style.fixedHeightPx, 1f);
            shiftX = Mathf.Clamp(shiftX, -maxShiftX, maxShiftX);
            shiftY = Mathf.Clamp(shiftY, -maxShiftY, maxShiftY);

            var margin = tmp.margin;
            margin.x += shiftX;
            margin.z -= shiftX;
            margin.y += shiftY;
            margin.w -= shiftY;
            tmp.margin = margin;
        }

        private static bool TryMeasureRenderedInsets(TMP_Text tmp, out Vector4 insets)
        {
            insets = Vector4.zero;
            if (tmp == null || tmp.rectTransform == null)
            {
                return false;
            }

            tmp.ForceMeshUpdate();
            var info = tmp.textInfo;
            if (info == null || info.characterInfo == null || info.characterInfo.Length == 0)
            {
                return false;
            }

            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;
            var visibleCount = 0;
            for (var i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible)
                {
                    continue;
                }

                visibleCount++;
                minX = Mathf.Min(minX, character.topLeft.x, character.bottomLeft.x, character.topRight.x, character.bottomRight.x);
                maxX = Mathf.Max(maxX, character.topLeft.x, character.bottomLeft.x, character.topRight.x, character.bottomRight.x);
                minY = Mathf.Min(minY, character.topLeft.y, character.bottomLeft.y, character.topRight.y, character.bottomRight.y);
                maxY = Mathf.Max(maxY, character.topLeft.y, character.bottomLeft.y, character.topRight.y, character.bottomRight.y);
            }

            if (visibleCount <= 0 || float.IsInfinity(minX) || float.IsInfinity(minY) || float.IsInfinity(maxX) || float.IsInfinity(maxY))
            {
                return false;
            }

            var rect = tmp.rectTransform.rect;
            insets = new Vector4(
                minX - rect.xMin,
                rect.yMax - maxY,
                rect.xMax - maxX,
                minY - rect.yMin);
            return true;
        }

        private static int CountVisibleCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private static string BuildScaleFixCacheKey(TMP_Text tmp, FigmaTextStyle style)
        {
            var fontName = tmp != null && tmp.font != null ? tmp.font.name : "null-font";
            return string.Join("|",
                fontName,
                style.fontSizePx.ToString("F3", CultureInfo.InvariantCulture),
                style.fixedWidthPx.ToString("F3", CultureInfo.InvariantCulture));
        }

        private static string BuildIndentedText(string text, float indentPx, bool escapeText)
        {
            var source = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            if (source.Length == 0)
            {
                return string.Empty;
            }

            if (Mathf.Abs(indentPx) <= 0.0001f)
            {
                return escapeText ? EscapeTmpText(source) : source;
            }

            var paragraphs = source.Split('\n');
            var sb = new StringBuilder(source.Length + paragraphs.Length * 48);
            var indentStr = indentPx.ToString("0.###", CultureInfo.InvariantCulture);

            for (var i = 0; i < paragraphs.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append('\n');
                }

                var paragraph = paragraphs[i] ?? string.Empty;
                if (paragraph.Length == 0)
                {
                    continue;
                }

                var payload = escapeText ? EscapeTmpText(paragraph) : paragraph;
                sb.Append("<indent=")
                    .Append(indentStr)
                    .Append("px><line-indent=0px>")
                    .Append(payload)
                    .Append("</line-indent></indent>");
            }

            return sb.ToString();
        }

        private static string EscapeTmpText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static void SetWordWrapping(TMP_Text tmp, bool enabled)
        {
            if (tmp == null)
            {
                return;
            }

            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            try
            {
                var textType = tmp.GetType();
                var wrappingModeProperty = textType.GetProperty("textWrappingMode", Flags);
                if (wrappingModeProperty != null && wrappingModeProperty.CanWrite && wrappingModeProperty.PropertyType.IsEnum)
                {
                    var enumType = wrappingModeProperty.PropertyType;
                    var preferredNames = enabled
                        ? new[] { "Normal", "Wrap", "PreserveWhitespace" }
                        : new[] { "NoWrap" };

                    foreach (var preferredName in preferredNames)
                    {
                        var exists = false;
                        var names = Enum.GetNames(enumType);
                        for (var i = 0; i < names.Length; i++)
                        {
                            if (!string.Equals(names[i], preferredName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            exists = true;
                            break;
                        }

                        if (!exists)
                        {
                            continue;
                        }

                        var enumValue = Enum.Parse(enumType, preferredName, true);
                        wrappingModeProperty.SetValue(tmp, enumValue);
                        return;
                    }
                }
            }
            catch
            {
            }

            try
            {
                var legacyProperty = tmp.GetType().GetProperty("enableWordWrapping", Flags);
                if (legacyProperty != null && legacyProperty.CanWrite && legacyProperty.PropertyType == typeof(bool))
                {
                    legacyProperty.SetValue(tmp, enabled);
                }
            }
            catch
            {
            }
        }

        private static void MarkLayoutDirty(TMP_Text tmp)
        {
            if (tmp == null)
            {
                return;
            }

            tmp.havePropertiesChanged = true;
            tmp.SetVerticesDirty();
            tmp.SetLayoutDirty();
        }
    }
}

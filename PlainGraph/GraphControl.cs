///////////////////////////////////////////////////////////////////////////////
// 
// PlainGraph
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//
// License: GNU Lesser General Public License (LGPLv3)
//
// Jing, Lu (lujing@unvell.com)
//
// Copyright (C) unvell.com, 2013. All Rights Reserved
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;

namespace Unvell.UIControl.PlainGraph {

    #region Graph Control Host
    public partial class GraphControl : Control {
        private PlainGraphType _graph_type = PlainGraphType.Line;

        /// <summary>
        /// Specify which one of Chart type should be displayed
        /// </summary>
        public PlainGraphType GraphType {
            get => _graph_type;
			set {
				if (_graph_type == value) return;
				_graph_type = value;

				if (_graph_type == _last_graph_type) return;
				Graph = PlainGraphFactory.create_plain_graph(_graph_type);

				Graph.Margin = _graph_margin;
				Graph.IsShowLegend = _is_show_legend;
				Graph.Bounds = ClientRectangle;
				Graph.DataSource = DataSource;
				_last_graph_type = _graph_type;
				Invalidate();
			}
        }

        private PlainGraphType _last_graph_type = PlainGraphType.Line;

		/// <summary>
        /// PlainGraph core object to render chart
        /// </summary>
        public PlainCommonGraph Graph { get; private set; } = new LineGraph();

		#region Settings

        private bool _is_anti_alias = true;

        [DefaultValue(true)]
        public bool IsAntiAlias {
            get => _is_anti_alias;
			set {
                _is_anti_alias = value;
                Invalidate();
            }
        }

        private bool _is_show_legend = true;

        /// <summary>
        /// Specify whether legend should be displayed 
        /// </summary>
        [DefaultValue(true)]
        public bool IsShowLegend {
            get => Graph.IsShowLegend;
			set {
                _is_show_legend = Graph.IsShowLegend = value;
                Graph.UpdateBounds(ClientRectangle);
                Invalidate();
            }
        }

		public bool isShowEntityName = true;

        /// <summary>
        /// Specify whether the name of data entity should be displayed
        /// </summary>
        [DefaultValue(false)]
        public bool IsShowEntityName {
            get => Graph.IsShowEntityName;
			set {
				if (isShowEntityName == value) return;
				isShowEntityName = Graph.IsShowEntityName = value;
				Invalidate();
			}
        }

        private bool _is_show_data_tip = false;

        /// <summary>
        /// Specify whether tip of data should be displayed
        /// </summary>
        [DefaultValue(false)]
        public bool IsShowDataTip {
            get => Graph.IsShowDataTip;
			set {
				if (_is_show_data_tip == value) return;
				_is_show_data_tip = Graph.IsShowDataTip = value;
				Invalidate();
			}
        }

        private bool _is_show_data_value = false;

        /// <summary>
        /// Specify whether value of data should be displayed
        /// </summary>
        [DefaultValue(false)]
        public bool IsShowDataValue {
            get => Graph.IsShowDataValue;
			set {
				if (_is_show_data_value == value) return;
				_is_show_data_value = Graph.IsShowDataValue = value;
				Invalidate();
			}
        }

        /// <summary>
        /// Font of x-axis ruler
        /// </summary>
        public Font XRulerFont {
            get => Graph.XRulerFont;
			set { Graph.XRulerFont = value; Invalidate(); }
        }

        /// <summary>
        /// Font of y-axis ruler
        /// </summary>
        public Font YRulerFont {
            get => Graph.YRulerFont;
			set { Graph.YRulerFont = value; Invalidate(); }
        }

        /// <summary>
        /// Font of legend
        /// </summary>
        public Font LegendFont {
            get => Graph.LegendFont;
			set { Graph.LegendFont = value; Invalidate(); }
        }

        private Padding _graph_margin;

        /// <summary>
        /// Margin for chart
        /// </summary>
        public Padding GraphMargin {
            get { return _graph_margin = Graph.Margin; }
            set {
                _graph_margin = Graph.Margin = value;
                Graph.UpdateBounds(ClientRectangle);
                Invalidate();
            }
        }
        #endregion

        #region Constructor
        public GraphControl() {
            InitializeComponent();

            BackColor = Color.White;
            DoubleBuffered = true;
        }
        #endregion

        protected override void OnCreateControl() {
            Graph.Bounds = ClientRectangle;
            base.OnCreateControl();
        }

        private DataSource _data_source;

        /// <summary>
        /// Data source to render chart
        /// </summary>
        public DataSource DataSource {
            get => _data_source;
			set {
                _data_source = value;
                Graph.DataSource = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pe) {
            Draw(pe.Graphics, pe.ClipRectangle);
        }

        /// <summary>
        /// Print chart. currenly print format is not supported
        /// </summary>
        /// <param name="doc"></param>
        public void Print(PrintDocument doc) {
            doc.PrintPage += (sender, e) => {
                Draw(e.Graphics, e.MarginBounds);
            };
        }

        internal void Draw(Graphics g, Rectangle clip) {
            Graph.Font = Font;

            var old_smoothing_mode = SmoothingMode.Default;
            if (_is_anti_alias) {
                old_smoothing_mode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            Graph.Draw(g, clip);
            if (_is_anti_alias) {
                g.SmoothingMode = old_smoothing_mode;
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            Graph.UpdateBounds(ClientRectangle);
            Invalidate();
        }
    }
    #endregion

    #region Common Graph
    public enum PlainGraphType {
        Line,
        LinePoint,
        LineArea,
        Column,
        StackedColumn,
        StackedPercentColumn,
        Pie,
        //Pie3D,
        //ExplodedPie,
    }

    public abstract class PlainCommonGraph {
        #region Border Attributes
        private Rectangle _bounds;

        public Rectangle Bounds {
            get => _bounds;
			set => UpdateBounds(value);
		}

		public Rectangle BorderBounds { get; set; }
		public Rectangle CaptionBounds { get; set; }
		public Rectangle GraphBounds { get; set; }
		public Rectangle LegendBounds { get; set; }
		public Padding Margin { get; set; }

		#endregion

        #region UI Attributes

		public Font Font { get; set; } = SystemFonts.DefaultFont;
		public Color CaptionColor { get; set; }
		public Font TitleFont { get; set; } = new Font(SystemFonts.DefaultFont.FontFamily, 14f, FontStyle.Bold);
		public Font XRulerFont { get; set; } = SystemFonts.DefaultFont;
		public Font YRulerFont { get; set; } = SystemFonts.DefaultFont;
		public Font LegendFont { get; set; } = SystemFonts.DefaultFont;
		public int LegendWidth { get; set; } = 80;

		#endregion

        #region Behavior Attributes

		public bool IsShowLegend { get; set; } = true;
		public bool IsShowEntityName { get; set; } = false;
		public bool IsShowDataTip { get; set; } = false;
		public bool IsShowDataValue { get; set; } = false;

		//private bool isShowPosAssistLineX = false;
        //private bool isShowPosAssistLineY = false;

        #endregion

        #region Constructor
        public PlainCommonGraph() {
        }

        public PlainCommonGraph(Rectangle bounds) {
            Bounds = bounds;
        }
        #endregion

        #region Data Attributes
        private DataSource _data_source;

        public DataSource DataSource {
            get => _data_source;
			set => UpdateDataSource(value);
		}

		public PlainGraphDisplayFormat KeyDisplayFormat { get; set; } = PlainGraphDisplayFormat.Integer;
		public PlainGraphDisplayFormat ValueDisplayFormat { get; set; } = PlainGraphDisplayFormat.Integer;

		protected DataInfo recordInfo = new DataInfo();
		public List<string> XDataKeys { get; set; } = new List<string>();
		public Dictionary<Color, string> Legends { get; set; } = new Dictionary<Color, string>();

		#endregion

        #region Update
        public virtual void UpdateBounds(Rectangle newBounds) {
            _bounds = newBounds;

            BorderBounds = new Rectangle(_bounds.Left + Margin.Left, _bounds.Top + Margin.Top,
                _bounds.Right - Margin.Right - Margin.Left, _bounds.Bottom - Margin.Bottom - Margin.Top);

            CaptionBounds = new Rectangle(BorderBounds.Left + 20, BorderBounds.Top + 10, BorderBounds.Width - 20, 24);

            if (IsShowLegend) {
                LegendBounds = new Rectangle(BorderBounds.Right - LegendWidth - 10,
                    CaptionBounds.Bottom + 10, LegendWidth,
                    BorderBounds.Height - CaptionBounds.Height - 10);
            } else {
                LegendBounds = Rectangle.Empty;
            }

            GraphBounds = new Rectangle(BorderBounds.Left + 10,
                CaptionBounds.Bottom + 10,
                BorderBounds.Width - LegendBounds.Width - 30,
                BorderBounds.Height - CaptionBounds.Bottom - 10);

            OnUpdateBounds(BorderBounds);
        }

        protected abstract void OnUpdateBounds(Rectangle bounds);

        public void UpdateDataSource(DataSource dataSource) {
            _data_source = dataSource;
            recordInfo = new DataInfo();

            // preprocess
            if (dataSource?.Records != null && dataSource.Records.Count != 0) {
                DataRecord max_set_record = null;
                var auto_find_set_keys = DataSource.XDataKeys == null || DataSource.XDataKeys.Count == 0;

                recordInfo.maxEntityCount = dataSource.Records.Max(r => r.Set?.Count() ?? 0);
                recordInfo.columnTotal = new double[recordInfo.maxEntityCount];
                recordInfo.columnMax = new double[recordInfo.maxEntityCount];
                recordInfo.columnMin = new double[recordInfo.maxEntityCount];

                recordInfo.recordCount = dataSource.Records.Count;
                recordInfo.recordTotal = new double[recordInfo.recordCount];
                recordInfo.recordMax = new double[recordInfo.recordCount];
                recordInfo.recordMin = new double[recordInfo.recordCount];

                if (DataSource.Records != null) {
                    for (var r = 0; r < DataSource.Records.Count; r++) {
                        var row = DataSource.Records[r];

                        if (auto_find_set_keys && (max_set_record == null || row.Set.Count > max_set_record.Set.Count)) max_set_record = row;

                        double record_total = 0;

                        for (var i = 0; i < row.Set.Count; i++) {
                            var entity = row.Set[i];

                            if ((this is PieGraph)) {
                                if (entity.Style == null) entity.Style = new DataEntityStyle();
                                if (entity.Style.Color.IsEmpty && (this is PieGraph)) {
                                    entity.Style.Color = PlainGraphToolkit.get_random_color();
                                }
                            }

                            record_total += entity.Value;

                            recordInfo.columnTotal[i] += entity.Value;
                            recordInfo.columnTotalMax = Math.Max(recordInfo.columnTotalMax, recordInfo.columnTotal[i]);

                            recordInfo.columnMax[i] = Math.Max(recordInfo.columnMax[i], entity.Value);
                            recordInfo.columnMin[i] = Math.Min(recordInfo.columnMin[i], entity.Value);

                            recordInfo.maxValue = Math.Max(recordInfo.maxValue, entity.Value);
                            recordInfo.minValue = Math.Min(recordInfo.minValue, entity.Value);
                        }

                        recordInfo.recordTotal[r] = record_total;
                        recordInfo.recordMax[r] = Math.Max(recordInfo.recordMax[r], record_total);
                        recordInfo.recordMin[r] = Math.Min(recordInfo.recordMin[r], record_total);

                        recordInfo.total += record_total;
                    }
                }

                if (auto_find_set_keys && max_set_record != null)
                    XDataKeys = (from r in max_set_record.Set select r.Key.ToString()).ToList();
                else
                    XDataKeys = DataSource.XDataKeys;

            } else {
                recordInfo.recordTotal = new double[0];
                recordInfo.recordCount = 0;
            }


            //UpdateBounds(bounds);
            OnUpdateDataSource();

            Legends.Clear();
            SelectLegends();
        }

        protected abstract void OnUpdateDataSource();
        protected abstract void SelectLegends();

        #endregion

        #region Draw

        public void DrawToImage(Image img, bool isAntiAlias) {
            var rect = new Rectangle(0, 0, img.Width, img.Height);
            UpdateBounds(rect);
            using (var g = Graphics.FromImage(img)) {
                if (isAntiAlias) {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                }
                Draw(g, rect);
            }
        }

        internal virtual void Draw(Graphics g, Rectangle clip) {
            if (_bounds.Width == 0 || _bounds.Height == 0) return;

            // draw border
            using (var p = new Pen(Color.Gray)) {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                p.Color = Color.Black;
                //g.DrawRectangle(p, borderBounds);

                p.Color = Color.Gray;
                //g.DrawRectangle(p, captionBounds);
                //g.DrawRectangle(p, graphBounds);
                //g.DrawRectangle(p, rowTitleBounds);
                //g.DrawRectangle(p, columnTitleBounds);
                //g.DrawRectangle(p, legendBounds);
            }

            DrawGraph(g);
            if (IsShowLegend) DrawLegend(g);

            if (_data_source != null && !string.IsNullOrEmpty(_data_source.Caption)) {
                g.DrawString(_data_source.Caption, TitleFont, Brushes.Black, CaptionBounds);
            }
        }

        protected abstract void DrawGraph(Graphics g);

        protected virtual void DrawLegend(Graphics g) {
            if (_data_source == null) return;

            var x = LegendBounds.Left;
            var total_height = 0;

            var item_rects = new Rectangle[Legends.Count];
            var color_rects = new Rectangle[Legends.Count];

            var i = 0;
            foreach (var c in Legends.Keys) {
                item_rects[i] = new Rectangle(LegendBounds.Left, 0, LegendBounds.Width, 20);

                var legend_str_size = g.MeasureString(Legends[c], Font, LegendBounds.Width - 20);
                var height = (int)legend_str_size.Height;
                if (height < 20) height = 20;
                item_rects[i].Height = height;
                total_height += height;

                color_rects[i] = new Rectangle(item_rects[i].Left, 0, 12, 12);
                i++;
            }

            i = 0;

            var y = LegendBounds.Top + (LegendBounds.Height - total_height) / 2;
            foreach (var c in Legends.Keys) {
                item_rects[i].Y = y;
                color_rects[i].Y = y;

                var text_rect = new Rectangle(color_rects[i].Right + 2, y, LegendBounds.Width, item_rects[i].Height);
                using (Brush b = new SolidBrush(c)) {
                    g.FillRectangle(b, color_rects[i]);
                }

                using (var sf = new StringFormat()) {
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Near;
                    g.DrawString(Legends[c], Font, Brushes.Black, text_rect);
                }

                y += item_rects[i].Height;
            }
        }
        #endregion

        #region Behavior
        public void OnMouseMove(MouseEventArgs e) {
            DoMouseMove(e);
        }

        public void OnMouseUp(MouseEventArgs e) {
        }

        public void OnMouseDown(MouseEventArgs e) {
        }

        internal virtual void DoMouseMove(MouseEventArgs e) { }
        internal virtual void DoMouseUp(MouseEventArgs e) { }
        internal virtual void DoMouseDown(MouseEventArgs e) { }
        #endregion
    }
    #endregion

    #region Utilities

    public struct DataInfo {
        internal int maxEntityCount;
        internal int recordCount;

        internal double[] columnTotal;
        internal double[] columnMax;
        internal double[] columnMin;

        internal double[] recordTotal;
        internal double[] recordMax;
        internal double[] recordMin;

        internal double total;
        internal double maxValue;
        internal double minValue;

        internal double columnTotalMax;

        internal double yRuleMax;
        internal double yRuleMin;
    }

    public class PlainGraphToolkit {
        public static readonly Color[] validColors = new[] { Color.Orange, Color.DarkBlue, Color.DarkGreen,
            Color.Magenta, Color.LimeGreen, Color.DimGray, Color.SkyBlue, Color.Pink, Color.RosyBrown};
        public static Color get_unused_color(DataSource dataSource) {
            var color = new List<Color>(validColors).FirstOrDefault(c => dataSource.Records.FirstOrDefault(r => r.Color == c) == null);
            if (color.IsEmpty) color = Color.Black;
            return color;
        }

        private static readonly Random rand = new Random();
        public static Color get_random_color() {
            return Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
        }
        public static Color get_random_dark_color() {
            return Color.FromArgb(rand.Next(200), rand.Next(200), rand.Next(200));
        }

        internal static int calc_fator(int v, int lvl) {
            var a = v.ToString().Length;
            var d = (v / lvl).ToString().Length;
            var b = (int)(Math.Pow(10, d - 1));
            var c = v / lvl;
            return (c - c % b);
        }
    }

    public enum PlainGraphDisplayFormat {
        Integer,
        Float2,
        Double,
        Percent,
    }

    #endregion

    #region CoordinateGraph

    public abstract class CoordinateGraph : PlainCommonGraph {
        public Point Origin { get; set; } = Point.Empty;
        public bool YAssistLine { get; set; } = true;
        public bool XAssistLine { get; set; } = true;

        #region Border Attributes

        public Rectangle RowTitleBounds { get; set; }
        public Rectangle ColumnTitleBounds { get; set; }

        #endregion

        #region Drawing

        protected override void DrawGraph(Graphics g) {
            // draw border
            //using (Pen p = new Pen(Color.Gray))
            //{
            //  p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            //  p.Color = Color.Blue;
            //  g.DrawRectangle(p, rowTitleBounds);
            //  g.DrawRectangle(p, columnTitleBounds);
            //}
            //MessageBox.Show(string.Format("DPIX: {0}%, DPIY: {1}%", g.DpiX *100/96f, g.DpiY*100/96f));

            var gb = GraphBounds;

            var x = 0;
            var y = 0;

            // x
            var interval = XDataKeys.Count == 0 ? gb.Width - 10 : (int)((gb.Width - 10) / (XDataKeys.Count));
            x = gb.Left + 5;
            y = ColumnTitleBounds.Top;
            for (var i = 0; i <= XDataKeys.Count; i++) {
                g.DrawLine(Pens.Red, x, gb.Bottom - 4, x, gb.Bottom);

                if (XAssistLine) {
                    using (Pen p = new Pen(Brushes.Silver)) {
                        p.DashStyle = DashStyle.Dot;
                        g.DrawLine(p, x, gb.Top, x, gb.Bottom - 5);
                    }
                }

                if (i >= XDataKeys.Count) continue;
                using (var sf = new StringFormat()) {
                    sf.Alignment = StringAlignment.Center;
                    //sf.FormatFlags |= StringFormatFlags.DirectionVertical;

                    var rect = new Rectangle(x, y, interval, ColumnTitleBounds.Height);
                    var str_size = g.MeasureString(XDataKeys[i], Font);
                    if (str_size.Width > rect.Width) {
                        if (i % 2 == 0) {
                            //rect.X -= rect.Width;
                            sf.Alignment = StringAlignment.Near;
                            rect.Width += interval;
                            g.DrawString(XDataKeys[i], XRulerFont, Brushes.Black, rect, sf);
                        }
                    } else
                        g.DrawString(XDataKeys[i], XRulerFont, Brushes.Black, rect, sf);
                }

                x += interval;
            }

            // y
            y = gb.Bottom - 5;
            x = RowTitleBounds.Right;
            var record_height = recordInfo.recordCount == 0 ? (GraphBounds.Height - 5) : (gb.Height - 10) / (recordInfo.recordCount);
            var row_height = (gb.Height - 10) / (Level - 1);
            for (var i = 0; i < Level; i++) {
                g.DrawLine(Pens.Red, gb.Left, y, gb.Left + 4, y);

                if (YAssistLine) {
                    using (var p = new Pen(Brushes.Silver)) {
                        p.DashStyle = DashStyle.Dot;
                        g.DrawLine(p, gb.Left + 6, y, gb.Right, y);
                    }
                }

                if (LevelValues.Count >= i + 1) {
                    var str = ((int)LevelValues[i]).ToString();

                    using (var sf = new StringFormat()) {
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString(str, YRulerFont, Brushes.Black, RowTitleBounds.Right, y - Font.Height / 2, sf);
                    }
                }

                y -= row_height;
            }

            g.DrawLine(Pens.Red, gb.Left + 5, gb.Top, gb.Left + 5, gb.Bottom);
            g.DrawLine(Pens.Red, gb.Left, gb.Bottom - 5, gb.Right, gb.Bottom - 5);

            // graph
            if (DataSource?.Records == null || DataSource.Records.Count <= 0) return;
            {
                for (var i = 0; i < recordInfo.recordCount; i++) {
                    draw_record(g, i, interval);
                }
            }
        }

        protected abstract void draw_record(Graphics g, int index, int interval);

        protected void draw_value(Graphics g, int x, int y, DataEntity entity) {
            using (var b = entity.Style == null ? Brushes.Black : new SolidBrush(entity.Style.Color)) {
                g.DrawString(entity.Value.ToString(CultureInfo.InvariantCulture), Font, b, x, y);
            }
        }

        #endregion

        #region Updating

        private const int RowTitleWidth = 40;
        private const int ColumnTitleHeight = 15;

        private int _col_width = 0;

        public int Level { get; set; } = 4;
        public List<double> LevelValues { get; set; } = new List<double>();

        protected override void OnUpdateBounds(Rectangle bounds) {
            RowTitleBounds = new Rectangle(bounds.Left + 10,
                CaptionBounds.Bottom + 10, RowTitleWidth,
                bounds.Height - CaptionBounds.Bottom - ColumnTitleHeight - 10);

            GraphBounds = new Rectangle(RowTitleBounds.Right + 2, CaptionBounds.Bottom + 10,
                bounds.Width - RowTitleBounds.Right - LegendBounds.Width - 10,
                bounds.Height - CaptionBounds.Bottom - ColumnTitleHeight - 10);

            LegendBounds = new Rectangle(bounds.Right - LegendWidth - 10,
                CaptionBounds.Bottom + 10, LegendWidth,
                bounds.Height - CaptionBounds.Bottom - 10);

            ColumnTitleBounds = new Rectangle(RowTitleBounds.Right + 2, GraphBounds.Bottom + 2,
                bounds.Width - RowTitleBounds.Right - LegendWidth - 10,
                bounds.Height - GraphBounds.Bottom);

            _col_width = ColumnTitleBounds.Width / (recordInfo.maxEntityCount == 0 ? 1 : recordInfo.maxEntityCount);
        }

        protected override void OnUpdateDataSource() {
            Origin = new Point(GraphBounds.Left, GraphBounds.Bottom);
            //recordInfo.yRuleMax = recordInfo.e;

            Level = 4;
            float f = 1;
            f = PlainGraphToolkit.calc_fator((int)get_ruler_y_max(), 4);
            if (f > 0) while (f * (Level - 1) < recordInfo.maxValue) Level++;

            LevelValues.Clear();
            for (var i = 0; i < Level; i++)
                LevelValues.Add(i * f);
            recordInfo.yRuleMax = LevelValues[Level - 1];

        }

        protected override void SelectLegends() {
            if (DataSource?.Records == null) return;
            foreach (var r in from r in DataSource.Records
                              select new { r.Color, Text = r.Key })
                Legends.Add(r.Color, r.Text);
        }

        protected abstract double get_ruler_y_max();

        #endregion

        #region Behavior
        //private ToolTip tip = new ToolTip();
        internal override void DoMouseMove(MouseEventArgs e) {
            if (IsShowDataTip) {
                //tip.Show(
            }
        }
        #endregion
    }

    #endregion

    #region PlainGraph Item

    public class PlainGraphText {
        public string Text { get; set; }
    }

    public class PlainGraphItem {
        public Rectangle Bounds { get; set; }
    }

    #endregion

    #region LineGraph

    public class LineGraph : CoordinateGraph {
        protected override void draw_record(Graphics g, int index, int interval) {
            var record = DataSource.Records[index];
            var bounds = GraphBounds;

            var cur_p = new Point(bounds.Left + 5 + interval / 2, bounds.Bottom);
            var last_p = Point.Empty;

            foreach (var entity in record.Set) {
                if (entity != null) {
                    cur_p.Y = bounds.Bottom - 5 - (recordInfo.yRuleMax == 0 ? 0 : (int)(entity.Value * bounds.Height / recordInfo.yRuleMax));

                    if (!last_p.IsEmpty)
                        using (var p = new Pen(record.Color)) {
                            p.Width = record.LineWeight;

                            if (entity.Style != null) {
                                if (entity.Style.EndCap != LineCap.Flat) {
                                    var min_b = Math.Min(interval, bounds.Height);

                                    switch (entity.Style.EndCap) {
                                        case LineCap.DiamondAnchor:
                                            p.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(min_b * 0.07f, min_b * 0.1f);
                                            break;
                                        case LineCap.Flat: break;
                                        case LineCap.Square: break;
                                        case LineCap.Round: break;
                                        case LineCap.Triangle: break;
                                        case LineCap.NoAnchor: break;
                                        case LineCap.SquareAnchor: break;
                                        case LineCap.RoundAnchor: break;
                                        case LineCap.ArrowAnchor: break;
                                        case LineCap.Custom: break;
                                        case LineCap.AnchorMask: break;
                                        default: throw new ArgumentOutOfRangeException();
                                    }
                                }

                                //p.EndCap = entity.Style.EndCap;
                                p.StartCap = entity.Style.StartCap;
                                p.DashStyle = entity.Style.LineStyle;

                            }
                            g.DrawLine(p, last_p, cur_p);
                        }

                    draw_point(g, entity, cur_p, record);
                }

                last_p = cur_p;
                cur_p.Offset(interval, 0);
            }
        }

        protected virtual void draw_point(Graphics g, DataEntity entity, Point p, DataRecord record) { }

        protected override double get_ruler_y_max() {
            return recordInfo.maxValue;
        }
    }

    #endregion

    #region LinePointGraph

    public class LinePointGraph : LineGraph {
		private const int PointSize = 5;

		protected override void draw_point(Graphics g, DataEntity entity, Point p, DataRecord record) {
            if (entity.Style != null && entity.Style.EndCap != LineCap.Flat) return;
            var size = (int)(record.LineWeight * 0.75f * PointSize);
            if (size < 5) size = 5;

            using (var sb = new SolidBrush(record.Color)) {
                g.FillEllipse(sb, p.X - size / 2, p.Y - size / 2, size, size);
            }
        }
    }

    #endregion

    #region LineAreaGraph

    public class LineAreaGraph : LineGraph {
        protected override void draw_record(Graphics g, int index, int interval) {
            var record = DataSource.Records[index];
            var bounds = GraphBounds;

            var curP = new Point(bounds.Left + 5 + interval / 2, bounds.Bottom);
            var points = new List<Point>();
            var minY = bounds.Bottom;

            foreach (var entity in record.Set) {
                if (entity != null) {
                    curP.Y = bounds.Bottom - 5 - (recordInfo.yRuleMax == 0 ? 0 : (int)(entity.Value * bounds.Height / recordInfo.yRuleMax));
                } else
                    curP.Y = bounds.Bottom;

                minY = Math.Min(curP.Y, minY);

                points.Add(curP);

                curP.Offset(interval, 0);
            }

            if (points.Count <= 1) return;
            points.Insert(0, new Point(bounds.Left + 5, bounds.Bottom - 5));
            points.Add(new Point(bounds.Right - 5, bounds.Bottom - 5));

            using (var path = new GraphicsPath()) {
                path.AddLines(points.ToArray());
                path.CloseAllFigures();

                var transparent_record_color1 = Color.FromArgb(50, ControlPaint.Light(record.Color));
                var transparent_record_color2 = Color.FromArgb(120, ControlPaint.Light(record.Color));
                using (var linear = new LinearGradientBrush(
                    new Rectangle(bounds.Left, minY, bounds.Width, bounds.Bottom - minY),
                    transparent_record_color1, transparent_record_color2, 90f)) {
                    g.FillPath(linear, path);
                }
            }

            using (var pen = new Pen(record.Color)) {
                g.DrawLines(pen, points.ToArray());
            }
        }
    }

    #endregion

    #region ColumnGraph

    public class ColumnGraph : CoordinateGraph {
        protected override void draw_record(Graphics g, int index, int interval) {
            var record = DataSource.Records[index];
            var rect = new Rectangle(GraphBounds.Left + 5, GraphBounds.Top, GraphBounds.Width - 10, GraphBounds.Height - 5);

            var offset_x = interval / 4;
            var col_width = (int)(interval * 0.5f / recordInfo.recordCount);

            for (var i = 0; i < record.Set.Count; i++) {
                var x = rect.Left + offset_x + i * interval + index * col_width;
                var entity = record.Set[i];

                var y = rect.Bottom - (int)(entity.Value * rect.Height / recordInfo.yRuleMax);

                using (var b = new SolidBrush(record.Color)) {
                    g.FillRectangle(b, x, y, col_width, rect.Bottom - y);
                }

                x += col_width;
            }
        }
        protected override double get_ruler_y_max() {
            return recordInfo.maxValue;
        }
    }

    #endregion

    #region StackedColumnGraph

    public class StackedColumnGraph : CoordinateGraph {
        protected override void draw_record(Graphics g, int rowIndex, int interval) {
            var record = DataSource.Records[rowIndex];
            var rect = new Rectangle(GraphBounds.Left + 5, GraphBounds.Top, GraphBounds.Width - 10, GraphBounds.Height - 5);
            int col_width = interval / 2;

            for (int i = 0; i < record.Set.Count; i++) {
                var x = (int)(rect.Left + i * interval + col_width - col_width * 0.375f);
                var entity = record.Set[i];

                var y = 0;
                for (var k = 0; k < rowIndex; k++)
                    y += (int)(DataSource.Records[k].Set[i].Value * rect.Height / recordInfo.columnTotalMax);

                var height = (int)(entity.Value * rect.Height / recordInfo.columnTotalMax);

                y = rect.Bottom - y - height;
                using (var b = new SolidBrush(record.Color)) {
                    g.FillRectangle(b, x, y, col_width * 0.75f, height);
                }
            }
        }
        protected override double get_ruler_y_max() {
            return recordInfo.columnTotalMax;
        }
    }

    public class StackedPercentColumnGraph : CoordinateGraph {
        protected override void draw_record(Graphics g, int rowIndex, int interval) {
            var record = DataSource.Records[rowIndex];

            var rect = new Rectangle(GraphBounds.Left + 5, GraphBounds.Top, GraphBounds.Width - 10, GraphBounds.Height - 5);

            var col_width = interval / 2;

            for (var i = 0; i < record.Set.Count; i++) {
                var x = (int)(rect.Left + i * interval + col_width - col_width * 0.375f);
                var entity = record.Set[i];

                var y = 0;
                for (var k = 0; k < rowIndex; k++)
                    y += (int)(DataSource.Records[k].Set[i].Value * rect.Height / recordInfo.columnTotal[i]);

                var height = (int)(entity.Value * rect.Height / recordInfo.columnTotal[i]);

                y = rect.Bottom - y - height;

                using (var b = new SolidBrush(record.Color)) {
                    g.FillRectangle(b, x, y, col_width * 0.75f, height);
                }
            }
        }
        protected override double get_ruler_y_max() {
            return recordInfo.columnTotalMax;
        }
    }

    #endregion

    #region Pie

    public class PieGraph : PlainCommonGraph {
        private Rectangle _pie_bounds = new Rectangle();

        protected override void OnUpdateBounds(Rectangle bounds) {
            var w = 10;
            var h = 7;

            float s = Math.Min(GraphBounds.Width / w, GraphBounds.Height / h);

            w = (int)(s * 10f) - 50;
            h = (int)(s * 7f) - 50;

            _pie_bounds = new Rectangle(GraphBounds.Left + (GraphBounds.Width - w) / 2,
                GraphBounds.Top + (GraphBounds.Height - h) / 2, w, h);
        }

        protected override void OnUpdateDataSource() {
        }

        protected override void SelectLegends() {
            if (DataSource?.Records == null || DataSource.Records.Count <= 0) return;
            foreach (var r in from r in DataSource.Records[0].Set
                              select new { Color = (r.Style.Color.IsEmpty ? PlainGraphToolkit.get_random_color() : r.Style.Color), Text = r.Key.ToString() })
                Legends.Add(r.Color, r.Text);
        }

        protected override void DrawGraph(Graphics g) {
            if (DataSource?.Records == null || DataSource.Records.Count == 0) return;

            var record = DataSource.Records[0];
            float angle = 0;

            foreach (var entity in record.Set) {
                var off = get_entity_angle(entity);
                draw_entity_pie(g, _pie_bounds, record, entity, angle, off);
                angle += off;
            }
        }

        private float get_entity_angle(DataEntity entity) {
            return (float)(entity.Value * 360f / recordInfo.recordTotal[0]);
        }

        protected virtual void draw_entity_pie(Graphics g, Rectangle pieBounds,
            DataRecord record, DataEntity entity, float startAngle, float endAngle) {
            if (pieBounds.Width <= 0 || pieBounds.Height <= 0) return;

            var color = entity.Style.Color;

            using (Brush b = new SolidBrush(color)) {
                g.FillPie(b, pieBounds, startAngle, endAngle);
            }

            draw_pie_string(g, pieBounds, record, entity, startAngle, endAngle);
        }

        protected virtual void draw_pie_string(Graphics g, Rectangle pieBounds,
            DataRecord record, DataEntity entity, float startAngle, float endAngle) {
            if (!IsShowEntityName) return;
            var w = pieBounds.Width / 3.5f;
            var h = pieBounds.Height / 3.5f;

            // angle -> radian
            var radian = (float)((startAngle + endAngle / 2) * Math.PI / 180f);
            var x = w * Math.Cos(radian);
            var y = h * Math.Sin(radian);

            if (x == double.NaN || y == double.NaN) return;
            var str = entity.Key.ToString(); //string.Format("{0}%", Math.Round(endAngle / 360f * 100f));
            var str_size = g.MeasureString(str, Font);

            g.DrawString(str,
                Font, Brushes.Black, pieBounds.Left + pieBounds.Width / 2 + (int)x - str_size.Width / 2,
                pieBounds.Top + pieBounds.Height / 2 + (int)y - str_size.Height / 2);
        }
    }

    #endregion

    #region Pie3DGraph

    public class Pie3DGraph : PieGraph {
        internal static PointF point_at_arc(PointF origin, RectangleF rect, float angle) {
            var radian = (float)((angle) * Math.PI / 180f);
            var x = (float)(origin.X + (rect.Width * Math.Cos(radian)));
            var y = (float)(origin.Y + (rect.Height * Math.Sin(radian)));

            return new PointF(x, y);
        }

        internal static float fixed_angle(float angle, RectangleF rect) {
            return (float)(180.0f / Math.PI * Math.Atan2(
                Math.Sin((angle) * Math.PI / 180f) * rect.Height / rect.Width,
                Math.Cos((angle) * Math.PI / 180f)));
        }

        protected override void draw_entity_pie(Graphics g, Rectangle pieBounds,
            DataRecord record, DataEntity entity, float startAngle, float endAngle) {
            if (pieBounds.Width <= 0 || pieBounds.Height <= 0) return;

            //if (record.Set.IndexOf(entity) != 4) return;

            var sa = startAngle + 2f;
            var ea = endAngle - 2f;

            var color = entity.Style.Color;

            var w = (int)(pieBounds.Width / 2f);
            var h = (int)(pieBounds.Height / 2f);

            var origin = new PointF(pieBounds.Left + w, pieBounds.Top + h);

            var origin_rect = new RectangleF(origin.X - 5, origin.Y - 5, 10, 10);
            var pto = point_at_arc(origin, origin_rect, sa + ea / 2);

            var half_rect = new RectangleF(origin.X - 5, origin.Y - 5, 10, 10);
            var pt1 = point_at_arc(pto, half_rect, sa);
            var pt2 = point_at_arc(pto, half_rect, sa + ea);

            //using (GraphicsPath gp = new GraphicsPath( FillMode.Winding))
            //{
            //  gp.AddLine(x1, y1, origial.X, origial.Y);
            //  gp.AddLine(origial.X, origial.Y, origial.X, origial.Y + 20);
            //  Rectangle rect = pieBounds;
            //  rect.Offset(0, 20);
            //  gp.AddArc(rect, sa + ea, -ea);

            //  gp.CloseAllFigures();

            //  using (Brush b = new SolidBrush(color))
            //  {
            //    //g.FillPath(b, gp);
            //  }
            //}

            //using (GraphicsPath gp = new GraphicsPath())
            //{
            //  gp.AddArc(pieBounds, sa, ea);
            //  gp.AddLine(origial.X, origial.Y, x1, y1);
            //  //g.DrawLine(Pens.Black, origial.X + x2, origial.Y + y2+20, origial.X, origial.Y);
            //  using (Brush b = new SolidBrush(ControlPaint.Light(color)))
            //  {
            //    //g.FillPath(b, gp);
            //  }
            //}

            var fixed_ang = fixed_angle(sa - 0.1f, half_rect);
            var fixed_end_angle = fixed_angle((sa + ea) - 0.1f, half_rect);

            var a2 = fixed_end_angle - fixed_ang;
            if (a2 < 0) a2 += 360;
            Debug.WriteLine("a2=" + a2);

            g.DrawLine(Pens.Black, pto.X, pto.Y, pt1.X, pt1.Y);
            g.DrawArc(Pens.Black, pieBounds, fixed_ang, a2);
            g.DrawLine(Pens.Black, pt2.X, pt2.Y, pto.X, pto.Y);

            var rect2 = pieBounds;
            rect2.Offset(0, 20);
            //g.DrawLine(Pens.Blue, origin.X, origin.Y, pt1.X, pt1.Y + 20);
            //g.DrawArc(Pens.Blue, rect2, fixedAngle, a2);
            //g.DrawLine(Pens.Blue, pt2.X, pt2.Y + 20, origin.X, origin.Y);


            draw_pie_string(g, pieBounds, record, entity, sa, ea);
        }
    }

    #endregion

    #region ExplodedPieGraph

    public class ExplodedPieGraph : PieGraph {
        protected override void draw_entity_pie(Graphics g, Rectangle pieBounds,
            DataRecord record, DataEntity entity, float startAngle, float endAngle) {
            if (pieBounds.Width <= 0 || pieBounds.Height <= 0) return;

            var color = entity.Style.Color;

            var w = pieBounds.Width * 1.2f;
            var h = pieBounds.Height * 1.2f;

            var radian = (float)((startAngle + endAngle / 2) * Math.PI / 180f);
            var x = w * Math.Cos(radian);
            var y = h * Math.Sin(radian);

            //if (record.Set.IndexOf(entity) == 3)
            //{
            //  //pieBounds.Inflate(-30, -30);
            //  //pieBounds = new Rectangle((int)x-pieBounds.Width/2, (int)y-pieBounds.Height/2, pieBounds.Width, pieBounds.Height);
            //}
            //else
            pieBounds.Inflate(-20, -20);

            using (Brush b = new SolidBrush(color)) {
                g.FillPie(b, pieBounds, startAngle, endAngle);
            }

            draw_pie_string(g, pieBounds, record, entity, startAngle, endAngle);
        }
    }

    #endregion

}

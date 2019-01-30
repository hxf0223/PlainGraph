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

using System.Collections.Generic;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Unvell.UIControl.PlainGraph {
    public class DataEntity {
        public string Key { get; set; }
        public double Value { get; set; }
        public string Comment { get; set; }
        public DataEntityStyle Style { get; set; }
    }


    public class DataRecord {
        public string Key { get; set; }
        public Color Color { get; set; } = Color.Blue;
        public float LineWeight { get; set; } = 1;
        public List<DataEntity> Set { get; set; } = new List<DataEntity>();

        public DataEntity add_data(string name, double value) {
            return add_data(name, value, Color.Empty);
        }

        public DataEntity add_data(string name, double value, Color color) {
            var entity = new DataEntity {
                Key = name,
                Value = value,
            };
            if (color != Color.Empty) {
                entity.Style = new DataEntityStyle {
                    Color = color,
                };
            }
            Set.Add(entity);
            return entity;
        }


        public DataRecord() { }

        public DataRecord(string name) {
            this.Key = name;
        }
    }

    public class DataSource {
        public string Caption { get; set; } = "PlainGraph";
        public string XTitle { get; set; }
        public string YTitle { get; set; }
        public List<string> XDataKeys { get; set; } = new List<string>();
        public List<DataRecord> Records { get; set; } = new List<DataRecord>();

        public DataRecord add_data(Dictionary<string, double> values) {
            return add_data(string.Empty, values, PlainGraphToolkit.get_unused_color(this));
        }
        public DataRecord add_data(string title, Dictionary<string, double> values) {
            return add_data(title, values, PlainGraphToolkit.get_unused_color(this));
        }
        public DataRecord add_data(string title, Dictionary<string, double> values, Color color) {
            var record = new DataRecord() {
                Key = title,
                Color = color,
            };

            foreach (var key in values.Keys) {
                var set = new DataEntity() {
                    Key = key,
                    Value = values[key],
                    Style = new DataEntityStyle {
                        Color = PlainGraphToolkit.get_random_color(),
                    },
                };
                record.Set.Add(set);
            }

            Records.Add(record);
            return record;
        }

        public DataRecord add_data(string title, IQueryable<KeyValuePair<string, int>> values, Color color) {
            var record = new DataRecord() {
                Key = title,
                Color = color,
            };

            foreach (var key in values.Select(v => v.Key)) {
                var set = new DataEntity() {
                    Key = key,
                    Value = values.FirstOrDefault(v => v.Key == key).Value,
                    Style = new DataEntityStyle {
                        Color = PlainGraphToolkit.get_random_color(),
                    },
                };
                record.Set.Add(set);
            }

            Records.Add(record);
            return record;
        }


        public DataRecord add_data(string title, Dictionary<int, double> values) {
            return add_data(title, values, PlainGraphToolkit.get_unused_color(this));
        }

        public DataRecord add_data(string title, Dictionary<int, double> values, Color color) {
            var record = new DataRecord {
                Key = title,
                Color = color,
            };

            foreach (var key in values.Keys) {
                var set = new DataEntity() {
                    Key = key.ToString(),
                    Value = values[key],
                    Style = new DataEntityStyle {
                        Color = PlainGraphToolkit.get_random_color(),
                    },
                };
                record.Set.Add(set);
            }

            Records.Add(record);
            return record;
        }

        public void add_data(DataRecord record) {
            Records.Add(record);
        }

        public DataRecord add_data(string name) {
            var record = new DataRecord(name);
            add_data(record);
            return record;
        }

        public DataSource() { }
        public DataSource(string name) { Caption = name; }
    }

    public class DataEntityStyle {
        public Color Color { get; set; }
        public DashStyle LineStyle { get; set; }
        public LineCap EndCap { get; set; }
        public LineCap StartCap { get; set; }
    }
}
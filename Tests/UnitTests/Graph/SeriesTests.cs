﻿namespace UnitTests.Graph
{
    using APSIM.Shared.Utilities;
    using Models;
    using Models.Core;
    using Models.Core.Run;
    using Models.Factorial;
    using Models.Graph;
    using Models.Storage;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using static UnitTests.Graph.MockSimulationDescriptionGenerator;

    [TestFixture]
    class SeriesTests
    {
        /// <summary>Create a single xy serie definition with no 'VaryBy' groupings.</summary>
        [Test]
        public void SeriesWithNoVaryBy()
        {
            var sim = new Simulation()
            {
                Name = "Sim1",
                Children = new List<Model>()
                {
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2"
                    },
                }
            };
            Apsim.ParentAllChildren(sim);

            string data =
                "CheckpointName  Col1  Col2\r\n" +
                "            ()    ()   (g)\r\n" +
                "       Current     1    10\r\n" +
                "       Current     1    10\r\n" +
                "       Current     2    20\r\n" +
                "       Current     2    20\r\n";

            var reader = new TextStorageReader(data);

            var series = sim.Children[0] as Series;
            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 1);
            Assert.AreEqual(definitions[0].xFieldName, "Col1");
            Assert.AreEqual(definitions[0].yFieldName, "Col2");
            Assert.AreEqual(definitions[0].colour, series.Colour);
            Assert.IsNull(definitions[0].error);
            Assert.IsNull(definitions[0].Filter);
            Assert.AreEqual(definitions[0].line, LineType.Solid);
            Assert.AreEqual(definitions[0].lineThickness, LineThicknessType.Normal);
            Assert.AreEqual(definitions[0].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[0].markerSize, MarkerSizeType.Normal);
            Assert.AreEqual(definitions[0].showInLegend, false);
            Assert.AreEqual(definitions[0].title, "Series");
            Assert.AreEqual(definitions[0].type, SeriesType.Bar);
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 1, 2, 2 });
            Assert.AreEqual(definitions[0].y as double[], new int[] { 10, 10, 20, 20 });
            Assert.IsNull(definitions[0].x2);
            Assert.IsNull(definitions[0].y2);
            Assert.AreEqual(definitions[0].xAxis, Axis.AxisType.Bottom);
            Assert.AreEqual(definitions[0].yAxis, Axis.AxisType.Left);
            Assert.AreEqual(definitions[0].xFieldUnits, "()");
            Assert.AreEqual(definitions[0].yFieldUnits, "(g)");
        }

        /// <summary>Create two series definitions due to a single 'VaryBy' grouping.</summary>
        [Test]
        public void SeriesWithOneVaryBy()
        {
            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "Exp", "Exp1"),
                        new Description("Sim1", "Exp", "Exp2")
                    }),
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2",
                        FactorToVaryColours = "Exp"
                    },
                }
            };
            Apsim.ParentAllChildren(folder);

            string data =
                "CheckpointName     Exp Col1  Col2\r\n" +
                "            ()      ()   ()   (g)\r\n" +
                "       Current    Exp1    1    10\r\n" +
                "       Current    Exp1    1    10\r\n" +
                "       Current    Exp2    2    20\r\n" +
                "       Current    Exp2    2    20\r\n";

            var reader = new TextStorageReader(data);

            var series = folder.Children[1] as Series;
            var descriptors = series.GetDescriptorNames().ToList();
            Assert.AreEqual(descriptors[0], "Exp");

            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 2);
            foreach (var definition in definitions)
            {
                Assert.AreEqual(definitions[0].xFieldName, "Col1");
                Assert.AreEqual(definitions[0].yFieldName, "Col2");
                Assert.IsNull(definitions[0].error);
                Assert.IsNull(definitions[0].Filter);
                Assert.AreEqual(definitions[0].line, LineType.Solid);
                Assert.AreEqual(definitions[0].lineThickness, LineThicknessType.Normal);
                Assert.AreEqual(definitions[0].marker, MarkerType.FilledCircle);
                Assert.AreEqual(definitions[0].markerSize, MarkerSizeType.Normal);
                Assert.AreEqual(definitions[0].showInLegend, false);
                Assert.AreEqual(definitions[0].type, SeriesType.Bar);
                Assert.IsNull(definitions[0].x2);
                Assert.IsNull(definitions[0].y2);
                Assert.AreEqual(definitions[0].xAxis, Axis.AxisType.Bottom);
                Assert.AreEqual(definitions[0].yAxis, Axis.AxisType.Left);
                Assert.AreEqual(definitions[0].xFieldUnits, "()");
                Assert.AreEqual(definitions[0].yFieldUnits, "(g)");
            }

            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].title, "Exp1");
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 1 });
            Assert.AreEqual(definitions[0].y as double[], new int[] { 10, 10 });

            Assert.AreEqual(definitions[1].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[1].title, "Exp2");
            Assert.AreEqual(definitions[1].x as double[], new double[] { 2, 2 });
            Assert.AreEqual(definitions[1].y as double[], new int[] { 20, 20 });
        }

        /// <summary>Create four series definitions due to a two 'VaryBy' groupings.</summary>
        [Test]
        public void SeriesWithTwoVaryBy()
        {
            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "Irr", "Dry", "Fert", "0"),
                        new Description("Sim1", "Irr", "Dry", "Fert", "10"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "0"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "10")
                    }),
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2",
                        FactorToVaryColours = "Irr",
                        FactorToVaryLines = "Fert"
                    },
                }
            };
            Apsim.ParentAllChildren(folder);

            string data =
                "CheckpointName     Irr  Fert   Col1  Col2\r\n" +
                "            ()      ()    ()   ()   (g)\r\n" +
                "       Current     Dry     0   1    10\r\n" +
                "       Current     Dry     0   2    20\r\n" +
                "       Current     Dry    10   1    30\r\n" +
                "       Current     Dry    10   2    40\r\n" +
                "       Current     Wet     0   1    50\r\n" +
                "       Current     Wet     0   2    60\r\n" +
                "       Current     Wet    10   1    70\r\n" +
                "       Current     Wet    10   2    80\r\n";

            var reader = new TextStorageReader(data);

            var series = folder.Children[1] as Series;
            var descriptors = series.GetDescriptorNames().ToList();
            Assert.AreEqual(descriptors[0], "Irr");
            Assert.AreEqual(descriptors[1], "Fert");

            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 4);
            foreach (var definition in definitions)
            {
                Assert.AreEqual(definitions[0].xFieldName, "Col1");
                Assert.AreEqual(definitions[0].yFieldName, "Col2");
                Assert.IsNull(definitions[0].error);
                Assert.IsNull(definitions[0].Filter);
                Assert.AreEqual(definitions[0].lineThickness, LineThicknessType.Normal);
                Assert.AreEqual(definitions[0].marker, MarkerType.FilledCircle);
                Assert.AreEqual(definitions[0].markerSize, MarkerSizeType.Normal);
                Assert.AreEqual(definitions[0].showInLegend, false);
                Assert.AreEqual(definitions[0].type, SeriesType.Bar);
                Assert.IsNull(definitions[0].x2);
                Assert.IsNull(definitions[0].y2);
                Assert.AreEqual(definitions[0].xAxis, Axis.AxisType.Bottom);
                Assert.AreEqual(definitions[0].yAxis, Axis.AxisType.Left);
                Assert.AreEqual(definitions[0].xFieldUnits, "()");
                Assert.AreEqual(definitions[0].yFieldUnits, "(g)");
            }

            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].line, LineType.Solid);
            Assert.AreEqual(definitions[0].title, "Dry0");
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[0].y as double[], new int[] { 10, 20 });

            Assert.AreEqual(definitions[1].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[1].line, LineType.Dash);
            Assert.AreEqual(definitions[1].title, "Dry10");
            Assert.AreEqual(definitions[1].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[1].y as double[], new int[] { 30, 40 });

            Assert.AreEqual(definitions[2].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[2].line, LineType.Solid);
            Assert.AreEqual(definitions[2].title, "Wet0");
            Assert.AreEqual(definitions[2].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[2].y as double[], new int[] { 50, 60 });

            Assert.AreEqual(definitions[3].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[3].line, LineType.Dash);
            Assert.AreEqual(definitions[3].title, "Wet10");
            Assert.AreEqual(definitions[3].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[3].y as double[], new int[] { 70, 80 });

        }

        /// <summary>Create six series definitions due to a three 'VaryBy' groupings.</summary>
        [Test]
        public void SeriesWithThreeVaryBy()
        {
            var data = "CheckpointName     Irr  Fert  Cultivar  Col1  Col2\r\n" +
                       "            ()      ()    ()        ()    ()   (g)\r\n" +
                       "       Current     Dry     0     Early     1    10\r\n" +
                       "       Current     Dry     0     Early     2    20\r\n" +
                       "       Current     Dry    20     Early     1    30\r\n" +
                       "       Current     Dry    20     Early     2    40\r\n" +
                       "       Current     Wet     0     Early     1    50\r\n" +
                       "       Current     Wet     0     Early     2    60\r\n" +
                       "       Current     Wet    20     Early     1    70\r\n" +
                       "       Current     Wet    20     Early     2    80\r\n" +
                       "       Current     Dry     0      Late     1    90\r\n" +
                       "       Current     Dry     0      Late     2    100\r\n" +
                       "       Current     Dry    20      Late     1    110\r\n" +
                       "       Current     Dry    20      Late     2    120\r\n" +
                       "       Current     Wet     0      Late     1    130\r\n" +
                       "       Current     Wet     0      Late     2    140\r\n" +
                       "       Current     Wet    20      Late     1    150\r\n" +
                       "       Current     Wet    20      Late     2    160\r\n";
            var reader = new TextStorageReader(data);

            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "Irr", "Dry", "Fert", "0", "Cultivar", "Early"),
                        new Description("Sim1", "Irr", "Dry", "Fert", "20", "Cultivar", "Early"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "0", "Cultivar", "Early"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "20", "Cultivar", "Early"),
                        new Description("Sim1", "Irr", "Dry", "Fert", "0", "Cultivar", "Late"),
                        new Description("Sim1", "Irr", "Dry", "Fert", "20", "Cultivar", "Late"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "0", "Cultivar", "Late"),
                        new Description("Sim1", "Irr", "Wet", "Fert", "20", "Cultivar", "Late")
                    }),
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2",
                        FactorToVaryColours = "Irr",
                        FactorToVaryLines = "Fert",
                        FactorToVaryMarkers = "Cultivar"
                    },
                }
            };
            Apsim.ParentAllChildren(folder);

            var series = folder.Children[1] as Series;
            var descriptors = series.GetDescriptorNames().ToList();
            Assert.AreEqual(descriptors[0], "Irr");
            Assert.AreEqual(descriptors[1], "Fert");
            Assert.AreEqual(descriptors[2], "Cultivar");

            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 8);

            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].line, LineType.Solid);
            Assert.AreEqual(definitions[0].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[0].title, "Dry0Early");
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[0].y as double[], new int[] { 10, 20 });

            Assert.AreEqual(definitions[1].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[1].line, LineType.Dash);
            Assert.AreEqual(definitions[1].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[1].title, "Dry20Early");
            Assert.AreEqual(definitions[1].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[1].y as double[], new int[] { 30, 40 });

            Assert.AreEqual(definitions[2].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[2].line, LineType.Solid);
            Assert.AreEqual(definitions[2].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[2].title, "Wet0Early");
            Assert.AreEqual(definitions[2].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[2].y as double[], new int[] { 50, 60 });

            Assert.AreEqual(definitions[3].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[3].line, LineType.Dash);
            Assert.AreEqual(definitions[3].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[3].title, "Wet20Early");
            Assert.AreEqual(definitions[3].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[3].y as double[], new int[] { 70, 80 });

            Assert.AreEqual(definitions[4].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[4].line, LineType.Solid);
            Assert.AreEqual(definitions[4].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[4].title, "Dry0Late");
            Assert.AreEqual(definitions[4].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[4].y as double[], new int[] { 90, 100 });

            Assert.AreEqual(definitions[5].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[5].line, LineType.Dash);
            Assert.AreEqual(definitions[5].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[5].title, "Dry20Late");
            Assert.AreEqual(definitions[5].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[5].y as double[], new int[] { 110, 120 });

            Assert.AreEqual(definitions[6].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[6].line, LineType.Solid);
            Assert.AreEqual(definitions[6].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[6].title, "Wet0Late");
            Assert.AreEqual(definitions[6].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[6].y as double[], new int[] { 130, 140 });

            Assert.AreEqual(definitions[7].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[7].line, LineType.Dash);
            Assert.AreEqual(definitions[7].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[7].title, "Wet20Late");
            Assert.AreEqual(definitions[7].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[7].y as double[], new int[] { 150, 160 });
        }

        /// <summary>
        /// Create series definitions where it works its way though the colours sequentially and 
        /// when it runs out of colours it works through the marker types. Useful when there
        /// are a lot of descriptor values.
        /// </summary>
        [Test]
        public void SeriesWithTwoIdenticalVaryBy()
        {
            var data = "CheckpointName   ABC   Col1  Col2\r\n" +
                       "            ()    ()     ()   (g)\r\n" +
                       "       Current     A      1    10\r\n" +
                       "       Current     A      2    20\r\n" +
                       "       Current     B      1    30\r\n" +
                       "       Current     B      2    40\r\n" +
                       "       Current     C      1    50\r\n" +
                       "       Current     C      2    60\r\n" +
                       "       Current     D      1    70\r\n" +
                       "       Current     D      2    80\r\n" +
                       "       Current     E      1    90\r\n" +
                       "       Current     E      2    100\r\n" +
                       "       Current     F      1    110\r\n" +
                       "       Current     F      2    120\r\n" +
                       "       Current     G      1    130\r\n" +
                       "       Current     G      2    140\r\n" +
                       "       Current     H      1    150\r\n" +
                       "       Current     H      2    160\r\n" +
                       "       Current     I      1    170\r\n" +
                       "       Current     I      2    180\r\n" +
                       "       Current     J      1    190\r\n" +
                       "       Current     J      2    200\r\n" +
                       "       Current     K      1    210\r\n" +
                       "       Current     K      2    220\r\n" +
                       "       Current     L      1    230\r\n" +
                       "       Current     L      2    240\r\n";
            var reader = new TextStorageReader(data);

            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "ABC", "A"),
                        new Description("Sim1", "ABC", "B"),
                        new Description("Sim1", "ABC", "C"),
                        new Description("Sim1", "ABC", "D"),
                        new Description("Sim1", "ABC", "E"),
                        new Description("Sim1", "ABC", "F"),
                        new Description("Sim1", "ABC", "G"),
                        new Description("Sim1", "ABC", "H"),
                        new Description("Sim1", "ABC", "I"),
                        new Description("Sim1", "ABC", "J"),
                        new Description("Sim1", "ABC", "K"),
                        new Description("Sim1", "ABC", "L"),
                    }),
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2",
                        FactorToVaryColours = "ABC",
                        FactorToVaryLines = "ABC",
                        FactorToVaryMarkers = "ABC"
                    }
                }
            };
            Apsim.ParentAllChildren(folder);

            var series = folder.Children[1] as Series;
            var descriptors = series.GetDescriptorNames().ToList();
            Assert.AreEqual(descriptors[0], "ABC");

            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 12);

            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[0].line, LineType.Solid);
            Assert.AreEqual(definitions[0].title, "A");

            Assert.AreEqual(definitions[1].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[1].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[1].line, LineType.Solid);
            Assert.AreEqual(definitions[1].title, "B");

            Assert.AreEqual(definitions[2].colour, ColourUtilities.Colours[2]);
            Assert.AreEqual(definitions[2].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[2].line, LineType.Solid);
            Assert.AreEqual(definitions[2].title, "C");

            Assert.AreEqual(definitions[3].colour, ColourUtilities.Colours[3]);
            Assert.AreEqual(definitions[3].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[3].line, LineType.Solid);
            Assert.AreEqual(definitions[3].title, "D");

            Assert.AreEqual(definitions[4].colour, ColourUtilities.Colours[4]);
            Assert.AreEqual(definitions[4].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[4].line, LineType.Solid);
            Assert.AreEqual(definitions[4].title, "E");

            Assert.AreEqual(definitions[5].colour, ColourUtilities.Colours[5]);
            Assert.AreEqual(definitions[5].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[5].line, LineType.Solid);
            Assert.AreEqual(definitions[5].title, "F");

            Assert.AreEqual(definitions[6].colour, ColourUtilities.Colours[6]);
            Assert.AreEqual(definitions[6].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[6].line, LineType.Solid);
            Assert.AreEqual(definitions[6].title, "G");

            Assert.AreEqual(definitions[7].colour, ColourUtilities.Colours[7]);
            Assert.AreEqual(definitions[7].marker, MarkerType.FilledCircle);
            Assert.AreEqual(definitions[7].line, LineType.Solid);
            Assert.AreEqual(definitions[7].title, "H");

            // Run out of colours, go back to first colour but increment markertype.

            Assert.AreEqual(definitions[8].colour, ColourUtilities.Colours[0]); 
            Assert.AreEqual(definitions[8].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[8].line, LineType.Solid);
            Assert.AreEqual(definitions[8].title, "I");

            Assert.AreEqual(definitions[9].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[9].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[9].title, "J");
            Assert.AreEqual(definitions[9].line, LineType.Solid);

            Assert.AreEqual(definitions[10].colour, ColourUtilities.Colours[2]);
            Assert.AreEqual(definitions[10].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[10].line, LineType.Solid);
            Assert.AreEqual(definitions[10].title, "K");

            Assert.AreEqual(definitions[11].colour, ColourUtilities.Colours[3]);
            Assert.AreEqual(definitions[11].marker, MarkerType.FilledDiamond);
            Assert.AreEqual(definitions[11].line, LineType.Solid);
            Assert.AreEqual(definitions[11].title, "L");

        }

        /// <summary>Create a xy series definitions with a regression annotation.</summary>
        [Test]
        public void SeriesWithChildRegressionAnnotation()
        {
            var sim = new Simulation()
            {
                Name = "Sim1",
                Children = new List<Model>()
                {
                    new Series()
                    {
                        Name = "Series",
                        TableName = "Report",
                        XFieldName = "Col1",
                        YFieldName = "Col2",

                        Children = new List<Model>()
                        {
                            new Regression()
                            {
                                showEquation = true,
                                showOneToOne = true
                            }
                        }
                    },
                }
            };
            Apsim.ParentAllChildren(sim);

            string data =
                "CheckpointName  Col1  Col2\r\n" +
                "            ()    ()   (g)\r\n" +
                "       Current     1    1.0\r\n" +
                "       Current     2    1.5\r\n" +
                "       Current     3    2.0\r\n" +
                "       Current     4    2.5\r\n";

            var reader = new TextStorageReader(data);

            var series = sim.Children[0] as Series;
            var definitions = new List<SeriesDefinition>();
            series.GetSeriesToPutOnGraph(reader, definitions);

            Assert.AreEqual(definitions.Count, 3);
            Assert.AreEqual(definitions[0].title, "Series");
            Assert.AreEqual(definitions[0].type, SeriesType.Bar);
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 2, 3, 4 });
            Assert.AreEqual(definitions[0].y as double[], new double[] { 1.0, 1.5, 2.0, 2.5 });

            Assert.AreEqual(definitions[1].title, "Regression line");
            Assert.AreEqual(definitions[1].type, SeriesType.Scatter);
            Assert.AreEqual(definitions[1].x as double[], new double[] { 1, 4 });
            Assert.AreEqual(definitions[1].y as double[], new double[] { 1, 2.5 });

            Assert.AreEqual(definitions[2].title, "1:1 line");
            Assert.AreEqual(definitions[2].type, SeriesType.Scatter);
            Assert.AreEqual(definitions[2].x as double[], new double[] { 1, 4 });
            Assert.AreEqual(definitions[2].y as double[], new double[] { 1, 4 });

        }

        /// <summary>Create series definitions with 'Vary by graph series'.</summary>
        [Test]
        public void SeriesWithVaryByGraphSeries()
        {
            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "Exp", "Exp1"),
                        new Description("Sim1", "Exp", "Exp2")
                    }),
                    new Graph()
                    {
                        Children = new List<Model>()
                        {
                            new Series()
                            {
                                Name = "Series1",
                                TableName = "Report",
                                XFieldName = "Col1",
                                YFieldName = "Col2",
                                FactorToVaryColours = "Graph series"
                            },
                            new Series()
                            {
                                Name = "Series2",
                                TableName = "Report",
                                XFieldName = "Col1",
                                YFieldName = "Col3",
                                FactorToVaryColours = "Graph series"
                            }
                        }
                    }
                }
            };
            Apsim.ParentAllChildren(folder);

            string data =
                "CheckpointName     Exp Col1  Col2  Col3\r\n" +
                "            ()      ()   ()   (g)    ()\r\n" +
                "       Current    Exp1    1    10    50\r\n" +
                "       Current    Exp1    1    10    50\r\n" +
                "       Current    Exp2    2    20    60\r\n" +
                "       Current    Exp2    2    20    60\r\n";

            var reader = new TextStorageReader(data);

            var series1 = folder.Children[1].Children[0] as Series;
            var definitions = new List<SeriesDefinition>();

            series1.GetSeriesToPutOnGraph(reader, definitions);
            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].title, "Series1");
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 1, 2, 2 });
            Assert.AreEqual(definitions[0].y as double[], new double[] { 10, 10, 20, 20 });


            var series2 = folder.Children[1].Children[1] as Series;
            var definitions2 = new List<SeriesDefinition>();
            series2.GetSeriesToPutOnGraph(reader, definitions2);
            Assert.AreEqual(definitions2[0].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions2[0].title, "Series2");
            Assert.AreEqual(definitions2[0].x as double[], new double[] { 1, 1, 2, 2 });
            Assert.AreEqual(definitions2[0].y as double[], new double[] { 50, 50, 60, 60 });
        }

        /// <summary>Create a single xy series definition with a 'Vary By Simulation' grouping.</summary>
        [Test]
        public void SeriesWithVaryBySimulation()
        {
            var folder = new Folder()
            {
                Name = "Folder",
                Children = new List<Model>()
                {
                    new MockSimulationDescriptionGenerator(new List<Description>()
                    {
                        new Description("Sim1", "SimulationName", "Sim1", "Exp", "Exp1"),
                        new Description("Sim1", "SimulationName", "Sim1", "Exp", "Exp2"),
                        new Description("Sim2", "SimulationName", "Sim2", "Exp", "Exp1"),
                        new Description("Sim2", "SimulationName", "Sim2", "Exp", "Exp2")
                    }),
                    new Graph()
                    {
                        Children = new List<Model>()
                        {
                            new Series()
                            {
                                Name = "Series1",
                                TableName = "Report",
                                XFieldName = "Col1",
                                YFieldName = "Col2",
                                FactorToVaryColours = "SimulationName"
                            }
                        }
                    }
                }
            };
            Apsim.ParentAllChildren(folder);

            string data =
                "CheckpointName  SimulationName    Exp Col1  Col2\r\n" +
                "            ()              ()     ()   ()   (g)\r\n" +
                "       Current            Sim1   Exp1    1    10\r\n" +
                "       Current            Sim1   Exp1    2    20\r\n" +
                "       Current            Sim2   Exp2    1    30\r\n" +
                "       Current            Sim2   Exp2    2    40\r\n";

            var reader = new TextStorageReader(data);

            var series1 = folder.Children[1].Children[0] as Series;
            var definitions = new List<SeriesDefinition>();

            series1.GetSeriesToPutOnGraph(reader, definitions);
            Assert.AreEqual(definitions.Count, 2);
            Assert.AreEqual(definitions[0].colour, ColourUtilities.Colours[0]);
            Assert.AreEqual(definitions[0].title, "Sim1");
            Assert.AreEqual(definitions[0].x as double[], new double[] { 1, 2 });
            Assert.AreEqual(definitions[0].y as double[], new double[] { 10, 20 });

            Assert.AreEqual(definitions[1].colour, ColourUtilities.Colours[1]);
            Assert.AreEqual(definitions[1].title, "Sim2");
            Assert.AreEqual(definitions[1].x as double[], new double[] { 1,  2 });
            Assert.AreEqual(definitions[1].y as double[], new double[] { 30, 40 });

        }

        /// <summary>Create some test data and return a storage reader. </summary>
        private static IStorageReader CreateTestData()
        {
            var data = "CheckpointName     Irr  Fert  Cultivar  Col1  Col2\r\n" +
                "            ()      ()    ()        ()    ()   (g)\r\n" +
                "       Current     Dry     0     Early     1    10\r\n" +
                "       Current     Dry     0     Early     2    20\r\n" +
                "       Current     Dry    20     Early     1    30\r\n" +
                "       Current     Dry    20     Early     2    40\r\n" +
                "       Current     Dry    40     Early     1    50\r\n" +
                "       Current     Dry    40     Early     2    60\r\n" +
                "       Current     Wet     0     Early     1    70\r\n" +
                "       Current     Wet     0     Early     2    80\r\n" +
                "       Current     Wet    20     Early     1    90\r\n" +
                "       Current     Wet    20     Early     2    100\r\n" +
                "       Current     Wet    40     Early     1    110\r\n" +
                "       Current     Wet    40     Early     2    120\r\n" +
                "       Current     Dry     0      Late     1    130\r\n" +
                "       Current     Dry     0      Late     2    140\r\n" +
                "       Current     Dry    20      Late     1    150\r\n" +
                "       Current     Dry    20      Late     2    160\r\n" +
                "       Current     Dry    40      Late     1    170\r\n" +
                "       Current     Dry    40      Late     2    180\r\n" +
                "       Current     Wet     0      Late     1    190\r\n" +
                "       Current     Wet     0      Late     2    200\r\n" +
                "       Current     Wet    20      Late     1    210\r\n" +
                "       Current     Wet    20      Late     2    220\r\n" +
                "       Current     Wet    40      Late     1    230\r\n" +
                "       Current     Wet    40      Late     2    240\r\n";
            return new TextStorageReader(data);
        }
    }
}

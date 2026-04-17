using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ASL;
using HexLib;

namespace ASLInputTool.ViewModels;

public partial class BoardEditorViewModel
{
    private void RefreshAllVisuals()
    {
        UpdateRoadVisuals();
        UpdateBuildingVisuals();
        UpdateCliffVisuals();
        UpdateHexsideTerrainVisuals();
    }

    private void UpdateRoadVisuals()
    {
        _roadVisuals.Clear();
        _waterVisuals.Clear();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData == null) continue;

                var neighborHexVm = _hexes.FirstOrDefault(h => h.Location.Equals(neighborLoc));
                Point destPoint;

                if (neighborHexVm != null)
                {
                    destPoint = new Point(neighborHexVm.CenterX, neighborHexVm.CenterY);
                }
                else
                {
                    // Boundary connection: use hexside midpoint
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };
                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];
                    destPoint = new Point((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
                }

                // 1. Water features (drawn below roads)
                if (edgeData.HasStream)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.DodgerBlue, Thickness = 4.0
                    });
                }
                if (edgeData.HasGully)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(210, 180, 140)), // Tan
                        Thickness = 6.0
                    });
                }
                if (edgeData.HasCanal)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.DodgerBlue, Thickness = 20.0
                    });
                }

                // 2. Roads
                if (edgeData.HasPavedRoad)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.DimGray, Thickness = 6.0
                    });
                }
                if (edgeData.HasDirtRoad)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(160, 110, 60)), // Lighter brown
                        Thickness = 6.0
                    });
                }
                if (edgeData.HasPath)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 80, 40)), // Darker brown for thin path
                        Thickness = 2.0
                    });
                }
            }
        }
    }

    private void UpdateBuildingVisuals()
    {
        if (_isUpdatingVisuals) return;
        _isUpdatingVisuals = true;

        try
        {
            var newList = new ObservableCollection<BuildingVisualBase>();
            var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();
            double squareSizeFactor = 0.5; // 50% of hex size as requested
            double connectorThicknessFactor = 0.75; // 75% of square size

            foreach (var hexVm in _hexes)
            {
                for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    int dirIndex = edgeIndex switch
                    {
                        0 => 0, // SE
                        1 => 5, // S
                        2 => 4, // SW
                        3 => 3, // NW
                        4 => 2, // N
                        5 => 1, // NE
                        _ => 0
                    };

                    var neighborLoc = hexVm.Location.GetNeighbor(dirIndex);
                    var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                    if (processedEdges.Contains(edgeKey)) continue;
                    processedEdges.Add(edgeKey);

                    var neighborHexVm = _hexes.FirstOrDefault(h => h.Location.Equals(neighborLoc));
                    if (neighborHexVm == null) continue;

                    var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                    if (edgeData != null && (edgeData.HasHouse || edgeData.IsRowhouse))
                    {
                        // Cleanup: If either hex is not a building, remove the house/rowhouse connection
                        if (!IsBuildingTerrain(hexVm.Terrain) || !IsBuildingTerrain(neighborHexVm.Terrain))
                        {
                            edgeData.HasHouse = false;
                            edgeData.IsRowhouse = false;
                            continue;
                        }

                        newList.Add(new BuildingConnectorViewModel
                        {
                            CanvasX = 0,
                            CanvasY = 0,
                            X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                            X2 = neighborHexVm.CenterX, Y2 = neighborHexVm.CenterY,
                            Thickness = _hexSize * squareSizeFactor * connectorThicknessFactor,
                            Fill = IsBuildingTerrain(hexVm.Terrain) ? GetBuildingBrush(hexVm.Terrain) : GetBuildingBrush(neighborHexVm.Terrain)
                        });

                        if (edgeData.IsRowhouse)
                        {
                            var p1 = hexVm.HexCorners[edgeIndex];
                            var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];
                            
                            // Calculate midpoint and direction of the hexside
                            double midX = (p1.X + p2.X) / 2.0;
                            double midY = (p1.Y + p2.Y) / 2.0;
                            double dx = p2.X - p1.X;
                            double dy = p2.Y - p1.Y;
                            double sideLen = Math.Sqrt(dx * dx + dy * dy);
                            
                            // Unit vector along the hexside
                            double ux = dx / sideLen;
                            double uy = dy / sideLen;
                            
                            // The line should be the width of the building connector
                            double lineHalfLen = (_hexSize * squareSizeFactor * connectorThicknessFactor) / 2.0;

                            newList.Add(new BuildingDividerViewModel
                            {
                                CanvasX = 0,
                                CanvasY = 0,
                                X1 = midX - ux * lineHalfLen, 
                                Y1 = midY - uy * lineHalfLen,
                                X2 = midX + ux * lineHalfLen, 
                                Y2 = midY + uy * lineHalfLen,
                                Thickness = 3.0 // Black divider line
                            });
                        }
                    }
                }
            }

            BuildingVisuals = newList;
        }
        finally
        {
            _isUpdatingVisuals = false;
        }
    }

    private bool IsBuildingTerrain(TerrainType terrain)
    {
        return terrain == TerrainType.StoneBuilding || 
               terrain == TerrainType.WoodenBuilding;
    }

    private Brush GetBuildingBrush(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.StoneBuilding => Brushes.DimGray,
            TerrainType.WoodenBuilding => Brushes.SaddleBrown,
            _ => Brushes.Transparent
        };
    }

    private (CubeCoordinate, CubeCoordinate) NormalizeEdge(CubeCoordinate a, CubeCoordinate b)
    {
        return (a.Q < b.Q || (a.Q == b.Q && a.R < b.R)) ? (a, b) : (b, a);
    }

    private void UpdateCliffVisuals()
    {
        var newList = new ObservableCollection<CliffVisualViewModel>();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData != null && edgeData.HasCliff)
                {
                    // Map direction index to visual hex corners
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };

                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];

                    newList.Add(new CliffVisualViewModel(CreateJaggedGeometry(p1, p2)));
                }
            }
        }
        CliffVisuals = newList;
    }

    private Geometry CreateJaggedGeometry(Point p1, Point p2)
    {
        var stream = new StreamGeometry();
        using (var context = stream.Open())
        {
            context.BeginFigure(p1, true, false);

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            
            // Perpendicular vector
            double nx = -dy / len;
            double ny = dx / len;

            int segments = 8;
            double toothSize = 4.0;

            for (int j = 1; j <= segments; j++)
            {
                double t = (double)j / segments;
                var currentP = new Point(p1.X + dx * t, p1.Y + dy * t);
                
                // Add a "tooth" at each segment
                var toothPoint = new Point(currentP.X + nx * toothSize, currentP.Y + ny * toothSize);
                context.LineTo(toothPoint, true, false);
                context.LineTo(currentP, true, false);
            }
        }
        stream.Freeze();
        return stream;
    }

    private void UpdateHexsideTerrainVisuals()
    {
        var newList = new ObservableCollection<HexsideTerrainVisualViewModel>();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData != null)
                {
                    // Map direction index to visual hex corners
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };

                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];

                    if (edgeData.HasWall)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.Gray, 5.0));
                    }
                    if (edgeData.HasHedge)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.ForestGreen, 5.0));
                    }
                    if (edgeData.HasBocage)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.DarkGreen, 8.0));
                    }
                }
            }
        }
        HexsideTerrainVisuals = newList;
    }

    private void UpdateRoadPreview(HexViewModel target)
    {
        RoadPreviewVisuals.Clear();
        if (RoadStartHex == null || RoadStartHex == target) return;

        int edgeIndex = GetNeighborEdgeIndex(RoadStartHex, target);
        if (edgeIndex != -1)
        {
            var p1 = new Point(RoadStartHex.CenterX, RoadStartHex.CenterY);
            var p2 = new Point(target.CenterX, target.CenterY);

            var visual = new RoadVisualViewModel
            {
                X1 = p1.X, Y1 = p1.Y,
                X2 = p2.X, Y2 = p2.Y,
                Stroke = GetActiveRoadBrush(),
                Thickness = GetActiveRoadThickness()
            };
            RoadPreviewVisuals.Add(visual);
        }
    }

    private Brush GetActiveRoadBrush()
    {
        return ActiveRoadType switch
        {
            RoadToolType.Paved => Brushes.DimGray,
            RoadToolType.Dirt => new SolidColorBrush(Color.FromRgb(160, 110, 60)),
            RoadToolType.Path => new SolidColorBrush(Color.FromRgb(120, 80, 40)),
            _ => Brushes.Black
        };
    }

    private double GetActiveRoadThickness()
    {
        return ActiveRoadType == RoadToolType.Path ? 2.0 : 6.0;
    }
}

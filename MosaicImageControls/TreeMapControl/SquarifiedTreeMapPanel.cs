using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace MosaicImageControls.TreeMapControl
{
    public class SquarifiedTreeMapPanel : TreeMapPanel
    {
        protected override Rect GetRectangle(RowOrientation orientation, TreeMapItem item, double x, double y, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                return new Rect(x, y, width, item.RealArea / width);
            else
                return new Rect(x, y, item.RealArea / height, height);
        }

        protected override void ComputeNextPosition(RowOrientation orientation, ref double xPos, ref double yPos, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                yPos += height;
            else
                xPos += width;
        }

        protected override void ComputeBounds()
        {
            this.Squarify(Items, new List<TreeMapItem>(), this.GetShortestSide());
        }

        private void Squarify(List<TreeMapItem> items, List<TreeMapItem> row, double sideLength)
        {
            if (items.Count == 0)
            {
                this.AddRowToLayout(row);
                return;
            }

            var item = items[0];
            var row2 = new List<TreeMapItem>(row);
            row2.Add(item);
            var items2 = new List<TreeMapItem>(items);
            items2.RemoveAt(0);

            double worst1 = this.Worst(row, sideLength);
            double worst2 = this.Worst(row2, sideLength);

            if (row.Count == 0 || worst1 > worst2)
                this.Squarify(items2, row2, sideLength);
            else
            {
                this.AddRowToLayout(row);
                this.Squarify(items, new List<TreeMapItem>(), this.GetShortestSide());
            }
        }

        private void AddRowToLayout(List<TreeMapItem> row)
        {
            base.ComputeTreeMaps(row);
        }

        private double Worst(List<TreeMapItem> row, double sideLength)
        {
            if (row.Count == 0) return 0;

            double maxArea = 0;
            double minArea = double.MaxValue;
            double totalArea = 0;
            foreach (var item in row)
            {
                maxArea = Math.Max(maxArea, item.RealArea);
                minArea = Math.Min(minArea, item.RealArea);
                totalArea += item.RealArea;
            }

            if (minArea == double.MaxValue) minArea = 0;

            double val1 = (sideLength * sideLength * maxArea) / (totalArea * totalArea);
            double val2 = (totalArea * totalArea) / (sideLength * sideLength * minArea);
            return Math.Max(val1, val2);
        }

    }
}

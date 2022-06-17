using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BingsuCodeEditor
{
    static class ExtensionMethods
    {

		#region DPI independence
		public static Rect TransformToDevice(this Rect rect, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return Rect.Transform(rect, matrix);
		}

		public static Rect TransformFromDevice(this Rect rect, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return Rect.Transform(rect, matrix);
		}

		public static Size TransformToDevice(this Size size, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return new Size(size.Width * matrix.M11, size.Height * matrix.M22);
		}

		public static Size TransformFromDevice(this Size size, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return new Size(size.Width * matrix.M11, size.Height * matrix.M22);
		}

		public static Point TransformToDevice(this Point point, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return new Point(point.X * matrix.M11, point.Y * matrix.M22);
		}

		public static Point TransformFromDevice(this Point point, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return new Point(point.X * matrix.M11, point.Y * matrix.M22);
		}
		#endregion

		#region System.Drawing <-> WPF conversions
		public static System.Drawing.Point ToSystemDrawing(this Point p)
		{
			return new System.Drawing.Point((int)p.X, (int)p.Y);
		}

		public static Point ToWpf(this System.Drawing.Point p)
		{
			return new Point(p.X, p.Y);
		}

		public static Size ToWpf(this System.Drawing.Size s)
		{
			return new Size(s.Width, s.Height);
		}

		public static Rect ToWpf(this System.Drawing.Rectangle rect)
		{
			return new Rect(rect.Location.ToWpf(), rect.Size.ToWpf());
		}
		#endregion

		public static IEnumerable<DependencyObject> VisualAncestorsAndSelf(this DependencyObject obj)
		{
			while (obj != null)
			{
				yield return obj;
				if (obj is Visual || obj is System.Windows.Media.Media3D.Visual3D)
				{
					obj = VisualTreeHelper.GetParent(obj);
				}
				else if (obj is FrameworkContentElement)
				{
					// When called with a non-visual such as a TextElement, walk up the
					// logical tree instead.
					obj = ((FrameworkContentElement)obj).Parent;
				}
				else
				{
					break;
				}
			}
		}
	}
}

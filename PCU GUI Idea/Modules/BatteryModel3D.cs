using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Controls;

namespace PCU_GUI_Idea.Modules
{
    public class BatteryModel3D
    {
        public void SetupScene(HelixViewport3D viewport)
        {
            // Create and add a Directional Light
            var directionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-1, -1, -1) // Light direction
            };



            var directionalLight2 = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(1, 1, 1) // Light direction
            };



            // Create and add a Point Light
            var pointLight = new PointLight
            {
                Color = Colors.White,
                Position = new Point3D(50, 50, 50)
            };



            // Create and add a Spot Light
            var spotLight = new SpotLight
            {
                Color = Colors.Yellow,
                Position = new Point3D(0, 5, 10),
                Direction = new Vector3D(0, 0, 0),
                InnerConeAngle = 30,
                OuterConeAngle = 45
            };



            // Add the lights to the viewport's `Children`
            viewport.Children.Add(new ModelVisual3D { Content = directionalLight });
            viewport.Children.Add(new ModelVisual3D { Content = directionalLight2 });
            // viewport.Children.Add(new ModelVisual3D { Content = pointLight });
            //viewport.Children.Add(new ModelVisual3D { Content = spotLight });
        }



        public void LoadModel(string filePath, HelixViewport3D viewport)
        {
            // Create a new FBX loader
            var importer = new ModelImporter();



            // Load the model
            Model3DGroup model = importer.Load(filePath) as Model3DGroup;
            // Add the model visual to the viewport
            viewport.Children.Add(new ModelVisual3D { Content = model });

        }

    }
}

# ALRadialMenu
A port of [Alex Littlejohn's][alex] ALRadialMenu https://github.com/AlexLittlejohn/ALRadialMenu

![gif][gif]

## Usage
The ALRadialMenu is fluent library, meaning all setter methods are chainable.

Initializing the menu

```
var radialMenu = 
    new ALRadialMenu()
		.SetButtons(buttons) // sets the button to display
		.SetDelay(0.125) // delay between animation for each button. Default 0
		.SetCircumference(90) // circumference of the menu. Default 360
		.SetRadius(66f) // radius or distance from the center. Default 100
		.SetDismissOnOverlayTap(true) // dismiss when tapping overlay. Default true
		.SetStartAngle(180); // angle where first button is drawn. Default 270
```

Displaying the menu in a view

```
radialMenu
    .SetAnimationOrigin(new CGRect(50, 100)) // where to display the menu
	.PresentInView(View); // which view to display it in
```

Displaying the menu in a window

```
radialMenu
	.PresentInWindow(View.Window); // which window to display it in
```

# License
Licensed under the MIT License. See LICENSE file for more information

[alex]: https://github.com/AlexLittlejohn/ALRadialMenu
[gif]: http://zippy.gfycat.com/BlandNaturalAnglerfish.gif

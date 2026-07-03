# Effect of Dim Lighting on Walking and Obstacle Navigation

## Obstacle Brightness Calibration

Unity task that sets the obstacle brightness so it is equally visible for every participant
under each lighting condition, controlling for individual differences in contrast sensitivity.
Each participant's calibrated shade is then used as their obstacle colour in the study.

## How it works

Adjust the obstacle shade with the arrow keys.

1. Shades run from dimmest to brightest; the participant selects the shade where the obstacle is barely visible.
2. The same shades run from brightest to dimmest; the participant again selects where it is barely visible.
3. The two responses are averaged to give an initial barely-visible shade.
4. The obstacle is shown at that average shade and lowered until the participant can no longer see it (final sahde).
5. The difference between the average shade and the final shade is added back onto the average to give the calibrated obstacle shade.

## Author

Danishta Kaul

<h1>STEELMEET TODO

## In Progress...
* All basic functionality Code Competition tab (All button should do something)
* Timer
* Undo lift
* Retry lift
* Something wrong with the order of updates for spectatorpanek suggetedbuttons runs an update all that breaks something with animation 

## Highest
* Visual Weights in info panels
* Right-click on attempt to open menu and change it's judging and weight

## High
* lifting order begin at 0 should begin at 1?
* Add button for exporting competitaion results inresults tab
* Exporting GL points decimal amount should be lowered to 2
* Warning when sending to comp if comp is loaded
* Crash when changing group of lifter and sending to comp if already in competition

#### Animations:
* Animation LifterInfo
* Animation Group sliding transition spectator panel
* Animation Datagrid judge color (pulse or boing)
* Animation Wheights in infopanels. Each weight go up then go on the bar then the lock at the end

## Medium
* Replace all messageboxes with a "pop-up" notification style thing that goes away after a couple of seconds
* Record attempt Spectator Window w/ epic effect
* Make themes selectable in settings
* Make TimerControl look better with selecting time
* Make all colors use the current theme
* Window only with Lifter info controls for spotters
* Custom window for Messagebox that correspond with the design of the rest of the program

## Low
* Wight amount and color settings in settings tab
* XAML Settings tab
* Code Settings tab
* Auto-save current settings to reg or file.
* Koeff classes rework?


## Backlog

**Import / Export / Auto-Save** Ongoing competitions


## Database Ideas
* Import / Export Weight-In´s
* Import / Export Results
* Import / Export 
* See class records while competing ( Toggelable )
* Auto record / personal best


## Done
* E SuggestedWeights
* E Rack heights need to be visable on the LifterInfoControl
* E Info panels control & spectator
* E Select next lifter
* E GL points never gets set in datagrid
* E est gl should only have 2 deciamls
* P Spectator window escape key wants to remove all newer spectator windows
* E Clicking in datagrid needs to change the selected lifter
* E Animation Lifter order
* E Spectator tab lifting order
* P Auto font size for spectator datagrid to max possible while still fitting all information
* E liftingOrder class where the liftingorder is handled & also updates visual lifting orders for control & spectator windows
* E JudgeControl Good or bad lifts (other buttons not done)
* E Simple RemoveLifter funtion in LiftingOrder class that will also handle animation of liftingorder
* E funcitons to update various stats in Lifter.cs after judging (RankUpdate, CalculateGLPoints, EstimatedUpdate, BestSBDUpdate)
* E funcitons to color specific cells in DataGrids
* E Switching groups clears and updates liftingOrder
* E ControlDgFormat to inherit from INotifyPropertyChanged for automatically sync Lifter with DataGrids
* E Spectator tab NextGroupLifterOrder
* E Make tabcontrol a new usecontrol instead of a just a style to hopefully fix visual bug
* E Dynamically animated Y-pos of next groups attempts with animation 
* P Drag move on spectator window 
* P Top right window buttons
* E Some usercontrols have a visable line between the control and the border
* E XAML Competition tab
* E XAML Spectator tab
* E In the space that sectator setting was previously put a lifter settings so you can change lifters weights and shit in another group
* E Make LifterInfo usercontrol fit better in spectatorpanel
* E F11 for fullscreen
* E Group switching com window
* E Use custom names for datagrid headers instead of varibe names
* E Highlightcolor on buttons checkboxes
* E Highlightcolor on checkboxes
* E XAML Weigh-In tab
* E Code Weigh-In tab
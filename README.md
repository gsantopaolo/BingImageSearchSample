PhotoAlbumSample
================

Goal of this sample is to show how to develop a UWP application covering all the
Windows 10 device families (with the right user experience for each family) by
implementing latest technology to make the user experience more personal and
more secure.

Concept of the app is allow the user to create a photo album with their own
picture, from his local storage or its own OneDrive, with picture found on the
web through an in app search on Bing Images

 

Aspect I want to cover: 
------------------------

### Create the right user experience for each usage. 

For doing that we need to understand if the user is interacting with Window 10
in three base ways:

-   Mouse and Keyboard

-   Tabled or Phones

-   LSX (Large Screen Experience such us Surface Hub)

The coolest part, once implemented, is that, if you have, for istance, an hybrid
laptop like the Surface Tablet/Book or the HP Spectre, if you are using the app
with mouse and keyboard and then you switch to tablet mode the app will show you
the same content on a totally different UI deigned ad hoc for you current usage!

Same if you are using the app on the phone and then you connect it on the
display dock (and I’m not talking only about relative panes and adaptive
triggers! ). Then when you run it on Surface Hub, your app will come alive and
it allows more users to work on different or same item at the same time!

This is, ihmo, a continuum app.

This part is almost done:

-   I’ve extended the WindowsStateTrigger with an InteractionCapabilityTrigger
    and Helper that let’s understand the actual InteractionCapability:

    -   MouseAndKeyboard

    -   SingleUserMultitouch

    -   MultiUserMultitouch

-   I’ve developed a ScatterView (old Surface SDK) control for UWP that it also
    bindable (go to look to sample code if interested). At [this
    link](<https://msdn.microsoft.com/en-us/library/ff727729.aspx>) you will
    find the concept of the Scatterview for LSX

-   Bing Image search engine is done, need to be re-engineered

-   Allow user to ink the right natural way (I’m going to implement the Ink
    toolbar)

-   Enable any text field to be filled with voice and ink (to be done)

-   Enable Hello/Passport api so the user can access its own OneDrive folder (to
    be done, I’ve already explored different ways and spent quite some time on
    that)

-   Show how to use the Windows.Devices.Perception to get stream from K4W and
    Realsense with the same code (I’ve some code that need to be reengineered
    and adapted to the scenario)

-   Enable drag and drop on different use cases

-   Part of the handled UI is missing

-   Part of the Surface UI is missing

-   Sharing the photo album (to be done)

Anyone in the willing of collaborate on that can ping me and I’ll add as member
on the git repo,

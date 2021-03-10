# MaterialFader
Fade specific properties of a material in and out using easing functions.

Example Setup:

![setup](https://user-images.githubusercontent.com/10963432/110688054-783fbd80-81e1-11eb-8f16-ef542a81a9d9.png)

In my case I was toying around with the new Malbers Realistic Wolf.
The eyes use the Standard material, property _EmissionColor as color. 
The fur is a special shader with a property _EmissionPower as float. 

Looks like this then:

![magic wolf](https://user-images.githubusercontent.com/10963432/110688835-4e3acb00-81e2-11eb-84cf-e16a9603710f.gif)

Note: Unity also has a working material lerp mechanism:

https://docs.unity3d.com/ScriptReference/Material.Lerp.html

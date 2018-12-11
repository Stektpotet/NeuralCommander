# NeuralCommander
 An AI project developed to learn about machine learning in a game setting, while also being inventive!
 
 
# Installation

## Executable installation

Extracts content of the release into a folder wherever.

2. Install latest python 3.6 (with pip!!!)
    1. add python + python/Scripts to env variables
3. Download [ml-agents-0.5.0a repo](https://github.com/Unity-Technologies/ml-agents.git)
4. install virtualenv, check with `virtualenv --version` first, it may already be installed:
```bash
   > pip3 install -U pip virtualenv
```
5. setup virtual env (see [tensorflow instructions](https://www.tensorflow.org/install/pip))
```bash
   C:\> virtualenv --system-site-packages -p python .\venv
   C:\> .\venv\Scripts\activate #activate the virtual environment for all the packages
```
6. install ml-agents' dependencies:
```bash
   > pip install -r some_path/ml-agents-0.5.0a/ml-agents/requirements.txt
```

## Project installation
1. Install [Unity 2018.2](https://store.unity.com/download) (through Unity Hub most versions of Unity are available)
2. Install latest python 3.6 (with pip!!!)
    1. add python + python/Scripts to env variables
3. Download [ml-agents-0.5.0a repo](https://github.com/Unity-Technologies/ml-agents.git)
4. install virtualenv, check with `virtualenv --version` first, it may already be installed:
```bash
   > pip3 install -U pip virtualenv
```
5. setup virtual env (see [tensorflow instructions](https://www.tensorflow.org/install/pip))
```bash
   C:\> virtualenv --system-site-packages -p python .\venv
   C:\> .\venv\Scripts\activate #activate the virtual environment for all the packages
```
6. install ml-agents' dependencies:
```bash
   > pip install -r some_path/ml-agents-0.5.0a/ml-agents/requirements.txt
```
7. install `mlagents-learn`
```
   > cd some_path/ml-agents-0.5.0a/ml-agents
   > c:\venv\Scripts\activate #activate the virtualenv
   > pip install . 		#results in the mlagents-learn command being available, YAY
```
8. Open the BootCamp Scene in unity
9. Open Player settings:  `Edit->Settings->Player`
10. set editor to use .Net 4.x equivalent
11. set editor "Scripting Defined Symbols": `ENABLE_TENSORFLOW, BOOTCAMP`
12. Download [TensorFlowSharp plugin for unity (link will download directly)](https://s3.amazonaws.com/unity-ml-agents/0.5/TFSharpPlugin.unitypackage)
13. import into your project (you will be prompted for this when doubleclicking the file while also having your unity project open)

14. from commandline (virtualenv activated [C:\>.\venv\Scripts\activate]):
```
   > mlagents-learn --help 	//validate that the installation is working
   > mlagents-learn ../config/trainer_config.yaml --run-id="Brain tag for the given run" --train //start trainer
```
when the message "INFO:mlagents.envs:Start training by pressing the Play button in the Unity Editor." appears, press play in the editor

# MORE
## TODO

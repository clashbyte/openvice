# OpenVice 

This is an OpenSource Grand Theft Auto: Vice City port to C#, which utilizes OpenTK for graphics, user IO and audio playing, BEPU Physics as a physics engine and a MP3Sharp lib for MP3 files decoding. 

Это - порт Grand Theft Auto: Vice City на C#, использующий OpenTK для вывода графики, захвата ввода пользователя и проигрывания аудио, BEPU Physics для просчёта физики и библиотеку MP3Sharp для декодинга MP3-файлов.

## License / Лицензия

First of all, this project nor team is not affliated with Rockstar Games, and we are not pretending to use game trade marks for something illegal. Consider this as a port of good old game to modern subsystems and graphic pipelines :D

Прежде всего, ни этот проект, ни команда никак не связаны с Rockstar Games, и мы не претендуем на использование торговых марок игры и компании-разработчика в каком-либо нелегальном виде. Считайте это портом старой-доброй игры с использованием современных технологий и графических возможностей :D

## How-to / Как собрать и запустить

To launch the engine, you obviously need an original Grand Theft Auto Vice City game distributive. Download main solution, build it and voila! Welcome to the sunniest city in all of the GTA franschise!

Для того чтобы собрать движок, Вам потребуется оригинальный дистрибутив Grand Theft Auto Vice City. Скачайте основное решение проекта, соберите его и вуаля! Добро пожаловать в самый солнечный город во всей GTA-франшизе!

## Code / Исходный код

Whole source code is commented with &lt;summary&gt; entries on two languages - English and Russian, so workflow of the engine must be pretty clear and obvious. Come on, dig inside and see game magic by your own eyes!

Весь исходный код закомментирован &lt;summary&gt;-вставками на двух языках - русском и английском, поэтому принцип работы движка должен быть прозрачным и понятным. Загляните внутрь и узрите всю магию своими глазами!

## Features / Преимущества

Keep in mind, that engine is being developed right now, and it's very young, so list of features are not too big.

* Parsing all of native GTA's RenderWare engine;
* Graphics subsystem is written using OpenGL, but you can write your own renderer and replace the OpenViceGL project (anyway, system is very-very extendable, everything is done by inheritance);
* Audio subsystem is powered by OpenAL and can play all of the SFX from original game. Even more - it can stream MP3 files and decode Vice City's ADF radio format;
* Open Source engine makes huge playground for modding and asset debugging.

Имейте ввиду, что движок на данный момент разрабатывается и ещё очень молод, поэтому преимуществ у него не так уж и много.

* Парсинг всех нативных форматов движка RenderWare оригинальной игры;
* Графическая подсистема использует OpenGL, но вы можете написать собственный рендерер и заменить проект OpenViceGL (как бы то ни было, система очень и очень гибкая, всё организовано через наследования);
* Аудио-подсистема базируется на OpenAL и может проигрывать любые звуковые эффекты оригинальной игры. Более того - она может стримить MP3-файлы и раскодировать ADF-файлы радиостанций;
* Движок с открытым исходным кодом открывает огромный потенциал для моддинга и дебага.

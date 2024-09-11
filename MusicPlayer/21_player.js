//歌曲清單相關
var audio = document.getElementById("audio");

//播放暫停相關
var play = document.getElementById("play");
var loop = document.getElementById("loop");
var mute = document.getElementById("mute")

//音量調整相關
var volControl = document.getElementById("volControl")
var volrange = volControl.children[0];
var vol = volControl.children[3];

//進度條相關
var proBar = document.getElementById("progressBar")

//播放時間相關
var info = document.getElementById("info")

//歌單用
var book = document.getElementById("book");
var bookSource = book.children[0];
var bookTarget = book.children[1];

//陣列用
var arr = [];

//////////////////////////////////////////////////////////////////////

//要在打開網頁時就馬上執行的function
musicArray(); // 進網頁時生出一個歌曲數量陣列，以便後面功能抓取使用 
setVolumebyrange(); //設定拉桿初始音量位置
InitMusicPool(); //預設將音樂本裡的歌丟進下拉選單

//進網頁時生出一個歌曲數量陣列，以便後面功能抓取使用
function musicArray() {
    for (var i = 0; i < bookSource.children.length; i++) {
        arr[i] = i;
    }
    console.log(arr);
}



//初始化音樂池
function InitMusicPool() {
    for (var i = 0; i < bookSource.children.length; i++) {
        var option = document.createElement("option");
        bookSource.children[i].id = "song" + (i + 1);
        bookSource.children[i].draggable = "true";
        bookSource.children[i].ondragstart = drag;

        option.value = bookSource.children[i].title;
        option.innerText = bookSource.children[i].innerText;

        songSelectList.appendChild(option);
    }
    changeMusic(0);
}

//====drag & drop的功能區=====

function allowDrop(ev) {
    ev.preventDefault();  //放棄物件預設行為
}

function drag(ev) {
    ev.dataTransfer.setData("a", ev.target.id);  //抓取正在拖曳的物件
}

function drop(ev) {
    ev.preventDefault();
    var data = ev.dataTransfer.getData("a");
    //console.log(ev.target);

    if (ev.target.id == "")
        ev.target.appendChild(document.getElementById(data));
    else
        ev.target.parentNode.appendChild(document.getElementById(data));
}



//更新音樂池
function updateMusicPool() {
    //清空音樂池
    for (var i = songSelectList.children.length - 1; i >= 0; i--) {
        songSelectList.remove(i);
    }


    //讀取bookTarget裡的歌給音樂池
    for (var i = 0; i < bookTarget.children.length; i++) {
        var option = document.createElement("option");

        option.value = bookTarget.children[i].title;
        option.innerText = bookTarget.children[i].innerText;

        songSelectList.appendChild(option);
    }
    changeMusic(0);
}

//////////////////////////////////////////////////////////////////////

//被點擊的那個物件
function eventTarget() {
    console.log(event.target);
}

//////////////////////////////////////////////////////////////////////

//使用者按下選單去換歌or用按鍵
function changeMusic(n) {
    var i = songSelectList.selectedIndex + n;


    //這邊在寫判斷
    //判斷如果循環歌單開著，那在最後一首＆第一首 按 下一首＆上一首 時，的狀況。
    //還有ran按鈕亮著時，按下一首要隨機。
    if (ran.src.includes("random-on.png")) {
        i = Math.floor(Math.random() * songSelectList.children.length);
    }
    else if (loop.src.includes("loop-YES.png") && i == songSelectList.children.length) {
        //換到第一首
        changeMusic(0 - songSelectList.selectedIndex);
    }
    else if (loop.src.includes("loop-YES.png") && i == -1) {
        //換到最後一首
        changeMusic(songSelectList.children.length - 1);
    } else { }

    audio.src = songSelectList.children[i].value;
    audio.title = songSelectList.children[i].innerText;
    songSelectList.children[i].selected = true; //用來標示children[i]是被選中的

    //選完後要在:播放中時，換下一首也自動播放。
    //若選完時，並沒有在播放中，那就換下一首但不播放。

    clearInterval(ProgressTackID);

    if (play.src.includes("pause.png")) {
        console.log("剛剛是播放中");
        audio.onloadeddata = playMusic;
    }
    else {
        console.log("剛剛是停止的");
        pauseMusic();
    }

}

//把progressBar.value變成0
function changeProgressBarValue() {
    proBar.value = parseInt(0);
    console.log(proBar.value);
}

//////////////////////////////////////////////////////////////////////

//播放時間
//重整抓取時間的格式
function musicFormat(t) {

    var m = parseInt(t / 60);
    var s = parseInt(t) % 60;
    //判斷是不是?是就給我執行、不是就給我閉嘴
    m = m < 10 ? "0" + m : m;
    s = s < 10 ? "0" + s : s;
    //把上面的東西丟下來，給我顯示這樣：
    return m + ":" + s;

}

function getMusicTimerForInfo() {

    //把上面的格式，讓currentTime跟duration套上去，然後回傳出上面指定的樣子m:s
    info.children[1].innerText = musicFormat(audio.currentTime) + " / " + musicFormat(audio.duration);

}

//////////////////////////////////////////////////////////////////////

var ProgressTackID;

//播放音樂
function playMusic() {
    audio.play();
    play.src = "images/pause.png";
    play.onclick = pauseMusic;

    progressBar.max = audio.duration * 10000; //按下播放後，自動把進度條的MAX改成這首歌的最大值

    // GyRA ID設置時沒有吧getMusicStatus帶進去
    ProgressTackID = setInterval(function () {
        getProgress();
        getMusicTimerForInfo();
        getMusicStatus();
    }, 1); //停幾秒後再執行一次

    info.children[0].innerText = "正在播放：" + audio.title;
}
//暫停音樂
function pauseMusic() {
    audio.pause();
    play.src = "images/play.png";
    play.onclick = playMusic;
    clearInterval(ProgressTackID);
    //getProgress(); 暫停可以不用再跑一次這個

    info.children[0].innerText = "正在播放：" + audio.title + "（暫停中）";

}
//音樂停止
function stopMusic() {
    pauseMusic();
    audio.currentTime = 0;
    clearInterval(ProgressTackID);
    getProgress(); //但停止要，因為要讓他馬上抓現在時間(00:00)，顏色、時間才會瞬間回到00:00。
    info.children[1].innerText = "00:00 / " + musicFormat(audio.duration);
    info.children[0].innerText = "正在播放：已停止";

}

//////////////////////////////////////////////////////////////////////

//製作一個可以判斷目前狀況的function
function getMusicStatus() {
    if (audio.currentTime == audio.duration) {
        if (ran.src.includes("random-on.png")) {
            //隨機選一首歌來播
            var r = Math.floor(Math.random() * songSelectList.children.length);
            r -= songSelectList.selectedIndex;
            changeMusic(r);
        }
        else if (loop.src.includes("loop-one.png")) {
            changeMusic(0);
        }
        else if (loop.src.includes("loop-YES.png") && songSelectList.selectedIndex == songSelectList.children.length - 1) {
            //換到第一首
            changeMusic(0 - songSelectList.selectedIndex);
        }
        else if (songSelectList.selectedIndex == songSelectList.children.length - 1) {
            stopMusic();
            changeMusic(0 - songSelectList.selectedIndex);
        }
        else { changeMusic(1); }
    }
}

//開啟單曲循環播放
function musicLoop() {
    loop.src = "images/loop-one.png";
    loop.onclick = onemusicLoop;
}
//開啟歌單循環-未完成
function onemusicLoop() {
    loop.src = "images/loop-YES.png";
    loop.onclick = musicunLoop;
}
//關閉循環播放
function musicunLoop() {
    loop.src = "images/loop-NO.png";
    loop.onclick = musicLoop;
}

//////////////////////////////////////////////////////////////////////

//想寫的random功能

function shuffle(array) {
    array.sort(() => Math.random() - 0.5);
    console.log(arr);
}


function randomMusic() {
    // loop.src = "images/loop-NO.png";
    ran.src = "images/random-on.png";
    // var r = Math.floor(Math.random() * songSelectList.children.length);
    // r -= songSelectList.selectedIndex;
    shuffle(arr);



    // changeMusic(r);
    ran.onclick = randomMusicOff;
}
//關閉隨機撥放
function randomMusicOff() {
    ran.src = "images/random-off.png";
    changeMusic(1);
    ran.onclick = randomMusic;
}

//////////////////////////////////////////////////////////////////////

//倒轉加速
function changeTime(t) {
    audio.currentTime += t;
}

//////////////////////////////////////////////////////////////////////

//音量調整
function setVolumebybutton(v) {

    volrange.value = parseInt(volrange.value) + v;
    setVolumebyrange();

    //用+-控制音量
    //audio.volume += v/100;
    //數字會跟著顯示目前音量
    //vol.value = parseInt(vol.value) + v;
}
//拉桿
function setVolumebyrange() {
    audio.volume = volrange.value / 100;
    vol.value = volrange.value;
    volrange.style.backgroundImage = `-webkit-linear-gradient(left, rgb(63, 91, 110) ${volrange.value}%, rgb(198, 202, 204) ${volrange.value}%)`;

}

//////////////////////////////////////////////////////////////////////

//靜音按鈕
function volMute() {

    audio.muted = !audio.muted;

    if (audio.muted)
        mute.src = "images/mute-ON.png"
    else mute.src = "images/mute-OFF.png"

}

//////////////////////////////////////////////////////////////////////

//進度條
//自動抓時間動
function getProgress() {
    //console.log(parseInt(audio.currentTime));
    //console.log('a');
    var w = audio.currentTime / audio.duration * 100;
    proBar.value = audio.currentTime * 10000; //讓bar的MAX 從秒變成毫秒，這樣進度在跑的時候會跑得比較小一格，不然原本MAX是100時，1秒一格很大一格。
    progressBar.style.backgroundImage = `-webkit-linear-gradient(left, rgb(63, 91, 110) ${w}%, rgb(198, 202, 204) ${w}%)`;

    //progressBar.value = audio[music].currentTime / audio[music].duration*progressBar.max;
    //console.log(parseInt(audio[music].currentTime) / parseInt(audio[music].duration) * 100); //用來在F12裡看的
}

//使用者拖曳時改時間
function setProgress() {
    audio.currentTime = progressBar.value / 10000;
}

//////////////////////////////////////////////////////////////////////


//控制歌單css display的元素
function showBook() {
    //console.log(window.getComputedStyle(book).display);
    book.style.display = (book.style.display == "flex" ? "none" : "flex");
    hideB.innerText = (hideB.innerText == "︾" ? "︽" : "︾");
}
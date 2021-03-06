// ml5.js: Object Detection with COCO-SSD (Webcam)
// The Coding Train / Daniel Shiffman
// https://thecodingtrain.com/learning/ml5/1.3-object-detection.html
// https://youtu.be/QEzRxnuaZCk

// p5.js Web Editor - Image: https://editor.p5js.org/codingtrain/sketches/ZNQQx2n5o
// p5.js Web Editor - Webcam: https://editor.p5js.org/codingtrain/sketches/VIYRpcME3
// p5.js Web Editor - Webcam Persistence: https://editor.p5js.org/codingtrain/sketches/Vt9xeTxWJ

// let img;
let video;
let detector;
let detections = [];
let object;

const widthC = 680;
const heightC = 480;

//set time
let totalSecond = 0;
let minutes = 0;
let seconds = 0;

//MQTT
const mqttTitle = "AceLab"


function preload() {
  // img = loadImage('dog_cat.jpg');
  detector = ml5.objectDetector('cocossd');
}

function gotDetections(error, results) {
  if (error) {
    console.error(error);
  }
  detections = results;
  detector.detect(video, gotDetections);
}

function setup() {
  //put canvas in div html
  var canvas = createCanvas(widthC, heightC);
  canvas.parent('canvasForHTML');

  video = createCapture(VIDEO);
  video.size(640, 480);
  video.hide();
  detector.detect(video, gotDetections);
}

function draw() {

  drawDetection();
  stroke("red");
  noFill();
  rect(widthC/4,20, widthC/2, heightC-20);

}

function drawDetection(){
  image(video, 0, 0);

  for (let i = 0; i < detections.length; i++) {
  // if(detections.length >0){
    object = detections[i];


    if(object.label=="person" || object.label == "cell phone" ||
      object.label == "teddy bear" || object.label == "wine glass"){


      stroke(0, 255, 0);
      strokeWeight(4);
      noFill();
      rect(object.x, object.y, object.width, object.height);
      noStroke();
      fill(255);
      textSize(24);
      text(object.label, object.x + 10, object.y + 24);
      
      text(object.label, object.x + 10, object.y + 24);
      if(object.label == "person"){
        if(object.x < widthC/4+widthC/4 && object.x > widthC/4-widthC/4){
          // console.log(performance.now());
          totalSecond++;
          minutes = Math.floor(totalSecond/3600);
          seconds = Math.floor((totalSecond - minutes *3600)/60);
          // milliseconds = totalSecond - (minutes*3600 + seconds*60);
          text(minutes + ":" + seconds, 190,50);

        }
        else{
          totalSecond = 0;
          minutes = 0;
          seconds = 0;
        }
        //console.log(minutes + ":" + seconds);
      }
      //control cell phone value
      
    client.publish(mqttTitle + '/Oculus/seconds', seconds.toString(), { qos: 0, retain: false });

    client.publish(mqttTitle +'/Oculus/minutes', minutes.toString(), { qos: 0, retain: false });
    
    
    
  }
}

    objectDetector('cell phone', 'cellPhone')
    objectDetector("teddy bear", "teddyBear")
    objectDetector("wine glass", "wineGlass")

}

  const objectDetector = (objectLabel,mqttName) => {

    let flag = false
    for (let i = 0; i < detections.length; i++) {
      // if(detections.length >0){
        object = detections[i];
    
    if(object.label === objectLabel){
      flag = true;
    }
    
    client.publish(mqttTitle +'/Oculus/' + mqttName, flag.toString(), { qos: 0, retain: false });

  }
  }




//Code for MQTT
const clientId = 'mqttjs_' + Math.random().toString(16).substr(2, 8)

const host = 'wss://mqtt.eclipseprojects.io:443/mqtt'
//const host = 'ws://134.122.33.147:9001/mqtt'

console.log('Connecting mqtt client')
const client = mqtt.connect(host)

client.on('error', (err) => {
console.log('Connection error: ', err)
client.end()
})

client.on('reconnect', () => {
console.log('Reconnecting...')
})

client.on('message', (topic, message, packet) => {
  console.log('Received Message: ' + message.toString() + '\nOn topic: ' + topic)
})





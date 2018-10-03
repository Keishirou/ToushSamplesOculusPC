#define SPEED1 170
#define SPEED2 255
#define JITTER 4

int slide;
int moving;
int flag;
int speed_level;
int diff;

void setup() {
  Serial.begin(38400);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // PWMによるスピード制御 (0-255)

  digitalWrite(12, HIGH);
  digitalWrite(9, HIGH);
  analogWrite(3, SPEED1);

  moving = 0;
  flag = 0;
  speed_level = SPEED1;
}

void loop() {  
  if ( Serial.available() ) {
    //key_move(str);

    String str = Serial.readStringUntil(';');
    slide = str.toInt();
    //Serial.println("1");

    if(slide >= 0){
      slide_device();
    }else{
      speed_change(slide);
    }
  }

  int v = analogRead(A2);
  Serial.println(v);
}

void slide_device(){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  if(vr > slide){
    // モーターA: 正転
    
    digitalWrite(12, HIGH);
    digitalWrite(9, LOW);
    //analogWrite(3, SPEED1);
    while(vr > slide){
      vr = analogRead(A2);
      diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){
        break;
      }
      if ( Serial.available() ) {
        String str = Serial.readStringUntil(';');
        slide = str.toInt();
      }
      delay(1);
    }  
    digitalWrite(9, HIGH);
    //analogWrite(3, SPEED1);
    /*
    analogWrite(3, 100);
    digitalWrite(12, HIGH);
    digitalWrite(9, LOW);
    while(vr > slide){
      vr = analogRead(A2);
      diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){
        break;
      }
      delay(1);
    }  
    digitalWrite(9, HIGH);
    */
  }else if(vr < slide){
    /*
    analogWrite(3, 100);
    while(vr < slide){
      vr = analogRead(A2);
      diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){
        break;
      }
       delay(1);
    }
  }
  */
    // モーターA: 逆転
    
    digitalWrite(12, LOW);
    digitalWrite(9, LOW);
    //analogWrite(3, SPEED2);
   while(vr < slide){
      vr = analogRead(A2);
      int diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){
        break;
      }
      if ( Serial.available() ) {
        String str = Serial.readStringUntil(';');
        slide = str.toInt();
      }
      delay(1);
    }
    digitalWrite(9, HIGH); 
    //analogWrite(3, SPEED2);
  }
}

void speed_change(int speedFlag){
  if(speedFlag == -1){
    speed_level = SPEED1;
  }else if(speedFlag == -2){
    speed_level = SPEED2;
  }
  analogWrite(3, speed_level);
}

void key_move(char mode){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  switch (mode) {
    case '0' : 
      digitalWrite(12, HIGH);
      digitalWrite(9, LOW);
      while(vr > 0){
        analogWrite(3, SPEED1);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      analogWrite(3, SPEED1);
      break;
    case '1' : 
      digitalWrite(12, LOW);
      digitalWrite(9, LOW); 
      while(vr < 512){
        analogWrite(3, SPEED1);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH);
      analogWrite(3, SPEED1); 
      break;
    case '2' :
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      break;
  }

  delay(10);
}


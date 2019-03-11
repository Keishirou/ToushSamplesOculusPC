#define SPEED1 240  // スライダを伸ばす場合（仮想物体を上向きになぞる場合）
#define SPEED2 140  // スライダを縮める場合
#define SPEED3 210  // スライダを押しとどめる場合（仮想物体を下向きになぞっている際にめり込んだ場合）
#define JITTER 10    // スライダが正しい位置に移動したかを判定する閾値

int slide;        // 受信したデータ（スライダを移動させる位置）
int upSpeed, downSpeed;  // 指定したモータの回転速度（現在未使用）
int diff;         // 指定されたスライダの位置と現在の位置の差
int preSlide;


void setup() {
  Serial.begin(38400);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // PWMによるスピード制御 (0-255)

  digitalWrite(12, HIGH);
  digitalWrite(9, HIGH);

  downSpeed = SPEED2;
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

  //int v = analogRead(A2);
  //Serial.println(v);
  //Serial.println(speed_level);
  Serial.println(1);
}

void slide_device(){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  if(vr > slide){     // モーターA: 正転（伸びる場合）
    digitalWrite(12, HIGH);
    digitalWrite(9, LOW);
    
    if(preSlide < slide){
      upSpeed = SPEED3;
    }
    else{
      upSpeed = SPEED1;
    }

    analogWrite(3, upSpeed);
    
    while(vr > slide){    // 受信したデータよりスライダの位置が大きい場合
      vr = analogRead(A2);
      diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
        break;
      }
      else if(diff < -(JITTER + 40)){   // めり込みすぎた場合に押し返す力を最大限にする
        //upSpeed = 255;
        analogWrite(3, 255);
      }
      if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
        String str = Serial.readStringUntil(';');
        preSlide = slide;
        slide = str.toInt();

        if(slide < 0){  // デバイスがめり込んだ状態で仮想物体から離れた場合
          break;
        }
      }
      delay(1);
    }  
    digitalWrite(9, HIGH);
  }else if(vr < slide){     // モーターA: 逆転（縮む場合）
    digitalWrite(12, LOW);
    digitalWrite(9, LOW);

    //downSpeed = SPEED2;
    analogWrite(3, downSpeed);
   
    while(vr < slide){    // 受信したデータよりスライダの位置が小さい場合
      vr = analogRead(A2);
      int diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
        break;
      }
      if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
        String str = Serial.readStringUntil(';');
        preSlide = slide;
        slide = str.toInt();

        if(slide < 0){  // デバイスがめり込んだ状態で仮想物体から離れた場合
          break;
        }
      }
      delay(1);
    }
    digitalWrite(9, HIGH); 
  }
  preSlide = slide;
}

void speed_change(int speedFlag){
  if(speedFlag == -1){
    digitalWrite(9, HIGH);
  }else if(speedFlag == -2){
    downSpeed = SPEED1;   // yama 181215 本来ならばここでスピードを変更すべきだががたつくため変更せず
  }else if(speedFlag == -3){
    downSpeed = SPEED2;
  }
}

//void key_move(char mode){
//  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
//  
//  switch (mode) {
//    case '0' : 
//      digitalWrite(12, HIGH);
//      digitalWrite(9, LOW);
//      while(vr > 0){
//        analogWrite(3, SPEED1);
//        vr = analogRead(A2);
//      }
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH); 
//      analogWrite(3, SPEED1);
//      break;
//    case '1' : 
//      digitalWrite(12, LOW);
//      digitalWrite(9, LOW); 
//      while(vr < 512){
//        analogWrite(3, SPEED1);
//        vr = analogRead(A2);
//      }
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH);
//      analogWrite(3, SPEED1); 
//      break;
//    case '2' :
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH); 
//      break;
//  }
//
//  delay(10);
//}

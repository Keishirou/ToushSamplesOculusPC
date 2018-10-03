#define SPEED1 240  // スライダを縮める場合
#define SPEED2 170  // スライダを高速で動かす場合（これが最高速）
#define JITTER 20    // スライダが正しい位置に移動したかを判定する閾値
#define MAX_LENGTH 1023

int slide;        // 受信したデータ（スライダを移動させる位置）
String str; //受信したデータの一時保存

int speed_level;  // 指定したモータの回転速度（現在未使用）
int diff;         // 指定されたスライダの位置と現在の位置の差


void setup() {
  //Serial.begin(9600);
 Serial.begin(38400);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // A PWMによるスピード制御 (0-255)
  digitalWrite(9, LOW);
}

void loop() {
Serial.flush();

  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  //
  //  // モーターA: 正転
  //  digitalWrite(12, HIGH);
  //  digitalWrite(9, LOW);
  //
  //  while(vr > 0){ //一番端っこ(抵抗値がMAX)に行くまで正転
  //    vr = analogRead(A2);
  ////    if(vr < MAX_LENGTH/2){
  ////      break;
  ////    }
  //    analogWrite(3, SPEED1);
  //    //Serial.println(vr);
  //  }
  //  digitalWrite(9, HIGH); //端っこに行ったらブレーキ
  //
  //  // 2秒間上記設定で回転
  //  delay(2000);
  //  Serial.println(vr);
  //
  //  // モーターA: 逆転
  //  digitalWrite(12, LOW);
  //  digitalWrite(9, LOW);
  //
  //  while(vr <MAX_LENGTH){ //一番端っこ(抵抗値が0側。電圧が高くなるほう)に行くまで逆転
  //    vr = analogRead(A2);
  //    analogWrite(3, SPEED1);
  //    //Serial.println(vr);
  //  }
  //  digitalWrite(9, HIGH); //端っこに行ったらブレーキ
  //
  //  // 2秒間上記設定で回転
  //  delay(2000);
//  String a = String(vr); 
//  String temp = a +",N";
  Serial.println(vr);
  //Serial.println(temp);
}

//シリアル割り込み処理
void serialEvent() {
  //Serial.println("serialEvent1");
  Serial.println("F");
  if (Serial.available() > 0) {
    str = Serial.readStringUntil(';');
    //str = Serial.readStringUntil('\n');
    slide = str.toInt();
    //slide = Serial.read();
    //Serial.println(slide);
    //delay(2000);
    //Serial.println("serialEvent2");
    //delay(2000);

    //ここに伸縮処理を入れてみる
    int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND

    if (vr > slide) {   // モーターA: 正転
      digitalWrite(12, HIGH);
      digitalWrite(9, LOW);
      analogWrite(3, SPEED1);

      while (vr > slide) {  // 受信したデータよりスライダの位置が大きい場合
        vr = analogRead(A2);
        diff = vr - slide;
        if (-JITTER < diff && diff < JITTER) { // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        /*割り込み処理で代替*/
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          slide = str.toInt();
          }
          
        //SlideFrag = true;
        delay(1);
        //Serial.println(vr);
      }
      digitalWrite(9, HIGH);
    } else if (vr < slide) {   // モーターA: 逆転
      digitalWrite(12, LOW);
      digitalWrite(9, LOW);
      analogWrite(3, SPEED2);

      while (vr < slide) {  // 受信したデータよりスライダの位置が小さい場合
        vr = analogRead(A2);
        int diff = vr - slide;
        if (-JITTER < diff && diff < JITTER) { // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        /*割り込み処理で代替*/
           if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
                String str = Serial.readStringUntil(';');
                slide = str.toInt();
              }
              
       // SlideFrag = true;
        delay(1);
        //Serial.println(vr);
      }
      digitalWrite(9, HIGH);
    }
Serial.flush();
    
  }
  //Serial.println("F");
}

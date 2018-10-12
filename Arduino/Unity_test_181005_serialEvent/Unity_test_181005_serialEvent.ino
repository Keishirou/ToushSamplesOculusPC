
#define SPEED1 240  // スライダを縮める場合
#define SPEED2 170
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
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  Serial.println(vr);
  //str = Serial.readStringUntil(';');
  //slide = str.toInt();
  //Serial.flush();
}

//シリアル割り込み処理
/*Unity側で行っている割り込み処理を重複させないための処理
  ArduinoがFを送信→Unity側からのシリアル通信を停止
  Arduinoがintを送信→Unity側からのシリアル通信を再開*/
void serialEvent() {

  Serial.println("F"); //割り込み処理を重複させないためのフラグ
  if (Serial.available() > 0) {
    str = Serial.readStringUntil(';');
    //Serial.flush();
    //str = Serial.readStringUntil('\n');
    //slide = Serial.read();
    slide = str.toInt();

    /*伸縮処理開始*/
    int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND

    if (vr > slide) {   // モーターA: 正転
      digitalWrite(12, HIGH); //正転
      digitalWrite(9, LOW); //ブレーキOFF
      analogWrite(3, SPEED1);

      while (vr > slide) {  // 受信したデータよりスライダの位置が大きい場合
        vr = analogRead(A2);
        diff = vr - slide;
        if (-JITTER < diff && diff < JITTER) { // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        /*割り込み処理で代替しようとしたけどこっちじゃないと即オーバーフローorガタガタ*/
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          //Serial.flush();
          slide = str.toInt();
        }
        delay(1);
      }
      digitalWrite(9, HIGH); //ブレーキON
    } else if (vr < slide) {   // モーターA: 逆転
      digitalWrite(12, LOW);  //逆転
      digitalWrite(9, LOW); //ブレーキOFF
      analogWrite(3, SPEED2);

      while (vr < slide) {  // 受信したデータよりスライダの位置が小さい場合
        vr = analogRead(A2);
        int diff = vr - slide;
        if (-JITTER < diff && diff < JITTER) { // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
    /*割り込み処理で代替しようとしたけどこっちじゃないと即オーバーフローorガタガタ*/
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          //Serial.flush();
          slide = str.toInt();
        }

        delay(1);
      }
      digitalWrite(9, HIGH);//ブレーキON
    }
  }
  //Serial.flush();
}

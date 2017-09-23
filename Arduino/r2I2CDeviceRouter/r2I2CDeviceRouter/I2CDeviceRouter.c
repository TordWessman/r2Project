
  /*
  Serial.println("input id, action, port, type");
  
  Serial.println(inp->id);
  Serial.println(inp->action);
  Serial.println(inp->args[REQUEST_ARG_CREATE_PORT_POSITION]);
  Serial.println(inp->args[REQUEST_ARG_CREATE_TYPE_POSITION]);
*/
  /*
  Serial.println("output id & action:");
  Serial.println(out.id);
  Serial.println(out.action);
  */
  
  /*  
    Serial.println(sizeof(ResponsePackage));  
  Serial.print("response Size:");
  Serial.println(responseSize);
  Serial.print("data size:");
  Serial.println(out.contentSize);
  */
  /*
    if (out.contentSize > 0 && Serial) {
  
    Serial.println("Content:");
    for (int i = 0; i < out.contentSize; i++) {Serial.print(out.content[i]); Serial.print(",");}
    Serial.println("");
  }
  */
  //Serial.print("data:");
  //Serial.println((const char*)out.content);
  
  /*
  
  delay(1000);
  byte port = 8;
  byte id = 0xA;
  byte create[] = {DEVICE_HOST_LOCAL, ACTION_CREATE_DEVICE , id, DEVICE_TYPE_ANALOGUE_INPUT, port};

  interpret((byte *)&create);


  Serial.println("Ready!");
  */
  
  /*
  byte port = 8;
  byte id = 0xA;
  byte get[] = {DEVICE_HOST_LOCAL, ACTION_GET_DEVICE , id};
  interpret((byte *)&get);
  
  */
  void a() {}

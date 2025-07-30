using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Xenon;

public enum ViewDirection
{
    Default,
    Front,
    Top,
    Side
}

public class Image_Manager : SingletonMono<Image_Manager>
{
    public float dimensionsMultiplier = 3f;
    string data_petrobras = "{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"C0000000968\",\r\n            \"assetUID\": \"C0000000968\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"HULL\",\r\n                    \"frame_Id\": \"1103132172\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"PLATE_18857\",\r\n                            \"plate_Id\": \"-1731777279\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-178.7623\",\r\n                                        \"y\": \"21.79253\",\r\n                                        \"z\": \"28.3\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"-1\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_16976\",\r\n                            \"plate_Id\": \"-306855515\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.4439\",\r\n                                        \"y\": \"20.11609\",\r\n                                        \"z\": \"28.3\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"-1\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"UpperDeck\",\r\n                    \"frame_Id\": \"1201463828\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"PLATE_18267\",\r\n                            \"plate_Id\": \"1526998326\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-165.6675\",\r\n                                        \"y\": \"29.38978\",\r\n                                        \"z\": \"20.0232\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"8.669637E-07\",\r\n                                        \"y\": \"0.995478\",\r\n                                        \"z\": \"0.09499305\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"TRANS BHD81\",\r\n                    \"frame_Id\": \"-1308185162\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"PLATE_8312\",\r\n                            \"plate_Id\": \"-1054334440\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-203.95\",\r\n                                        \"y\": \"15.37371\",\r\n                                        \"z\": \"21.06895\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}\r\n\r\n";

    //string data = "{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"16741282\",\r\n            \"assetUID\": \"16741282\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"Main Deck\",\r\n                    \"frame_Id\": \"1336775918\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Stiffener70-113-1\",\r\n                            \"plate_Id\": \"587773084\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"2.579708\",\r\n                                        \"y\": \"27.94367\",\r\n                                        \"z\": \"1.944837\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1.119568E-08\",\r\n                                        \"y\": \"-0.9999947\",\r\n                                        \"z\": \"-0.003242868\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate94\",\r\n                            \"plate_Id\": \"1746691204\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"2.424643\",\r\n                                        \"y\": \"27.93823\",\r\n                                        \"z\": \"-3.560982\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.0001453789\",\r\n                                        \"y\": \"-0.9999927\",\r\n                                        \"z\": \"0.003826968\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate219\",\r\n                            \"plate_Id\": \"-2079714119\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-3.543723\",\r\n                                        \"y\": \"27.94672\",\r\n                                        \"z\": \"1.004773\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1.119568E-08\",\r\n                                        \"y\": \"-0.9999947\",\r\n                                        \"z\": \"-0.003242868\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"FR 6\",\r\n                    \"frame_Id\": \"2073983494\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate1\",\r\n                            \"plate_Id\": \"-1735884712\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 4.762500286102295,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-4.5\",\r\n                                        \"y\": \"24.59319\",\r\n                                        \"z\": \"8.099826\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"FR 14\",\r\n                    \"frame_Id\": \"971744047\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate1\",\r\n                            \"plate_Id\": \"1756632421\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"5\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 19.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-10.5\",\r\n                                        \"y\": \"26.5065\",\r\n                                        \"z\": \"2.822407\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"8\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 19.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-10.5\",\r\n                                        \"y\": \"24.83094\",\r\n                                        \"z\": \"-1.547845\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"L51 Upper Eng Flat 21800 ABL\",\r\n                    \"frame_Id\": \"-621265125\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate1\",\r\n                            \"plate_Id\": \"-2082774159\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"6\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 4.762500286102295,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-3.694132\",\r\n                                        \"y\": \"21.79998\",\r\n                                        \"z\": \"1.306622\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9999999\",\r\n                                        \"z\": \"-3.576279E-07\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"ShipHull\",\r\n                    \"frame_Id\": \"427718721\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate94\",\r\n                            \"plate_Id\": \"-1934140841\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"7\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"8.058001\",\r\n                                        \"y\": \"24.30302\",\r\n                                        \"z\": \"-1.806147\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}\r\n\r\n";
    //string data = "{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"13943969\",\r\n            \"assetUID\": \"13943969\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"ShipHull\",\r\n                    \"frame_Id\": \"348046834\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate163\",\r\n                            \"plate_Id\": \"-363330063\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 26.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-115.3988\",\r\n                                        \"y\": \"0.001261835\",\r\n                                        \"z\": \"1.159473\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.0001045665\",\r\n                                        \"y\": \"0.9999998\",\r\n                                        \"z\": \"-0.0004831645\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate329\",\r\n                            \"plate_Id\": \"-1452224123\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 24.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-100.2478\",\r\n                                        \"y\": \"0.001048827\",\r\n                                        \"z\": \"-1.76351\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.0001738928\",\r\n                                        \"y\": \"0.9999999\",\r\n                                        \"z\": \"0.0001347949\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"L10 - LBHD Stbd\",\r\n                    \"frame_Id\": \"50972644\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate32\",\r\n                            \"plate_Id\": \"-1740640662\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-108.7904\",\r\n                                        \"y\": \"13.64923\",\r\n                                        \"z\": \"9.399989\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"-3.576279E-07\",\r\n                                        \"z\": \"-0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"FR 63\",\r\n                    \"frame_Id\": \"33302166\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate28\",\r\n                            \"plate_Id\": \"1764322061\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-82.7\",\r\n                                        \"y\": \"16.72344\",\r\n                                        \"z\": \"-2.923824\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"L10 - LBHD Port\",\r\n                    \"frame_Id\": \"-2012245970\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate1\",\r\n                            \"plate_Id\": \"-147694420\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"5\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-97.17808\",\r\n                                        \"y\": \"16.46678\",\r\n                                        \"z\": \"-9.4\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"-3.576279E-07\",\r\n                                        \"z\": \"-0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"Main Deck\",\r\n                    \"frame_Id\": \"965504023\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate422\",\r\n                            \"plate_Id\": \"-1058981628\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"6\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-123.9973\",\r\n                                        \"y\": \"27.94127\",\r\n                                        \"z\": \"-2.685692\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-2.15043E-08\",\r\n                                        \"y\": \"-0.9999947\",\r\n                                        \"z\": \"0.003243487\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}\r\n\r\n\r\n";
    //string data = "{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"13878122\",\r\n            \"assetUID\": \"13878122\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"ShipHull\",\r\n                    \"frame_Id\": \"718916820\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate511\",\r\n                            \"plate_Id\": \"1612909048\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-317.5104\",\r\n                                        \"y\": \"14.86841\",\r\n                                        \"z\": \"6.559446\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.6400968\",\r\n                                        \"y\": \"-0.1973619\",\r\n                                        \"z\": \"-0.7425122\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate7\",\r\n                            \"plate_Id\": \"-1937804504\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-319.8739\",\r\n                                        \"y\": \"17.65935\",\r\n                                        \"z\": \"3.321293\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.883664\",\r\n                                        \"y\": \"-0.1367784\",\r\n                                        \"z\": \"-0.4476936\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate56\",\r\n                            \"plate_Id\": \"1346071936\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-323.5751\",\r\n                                        \"y\": \"8.93411\",\r\n                                        \"z\": \"3.913868\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.4985666\",\r\n                                        \"y\": \"-0.03177678\",\r\n                                        \"z\": \"-0.8662688\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate500\",\r\n                            \"plate_Id\": \"46825106\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-318.8444\",\r\n                                        \"y\": \"16.01712\",\r\n                                        \"z\": \"-5.107372\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0.6453262\",\r\n                                        \"y\": \"-0.1733626\",\r\n                                        \"z\": \"0.7439755\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"Fr 108\",\r\n                    \"frame_Id\": \"1657867158\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate13\",\r\n                            \"plate_Id\": \"1337589679\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"5\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"17.94402\",\r\n                                        \"z\": \"-3.06719\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate31\",\r\n                            \"plate_Id\": \"-1794578201\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"6\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"18.44363\",\r\n                                        \"z\": \"1.50911\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate12\",\r\n                            \"plate_Id\": \"1337589678\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"7\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"12.17311\",\r\n                                        \"z\": \"1.185208\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate16\",\r\n                            \"plate_Id\": \"1337589674\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"8\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"9.061066\",\r\n                                        \"z\": \"-3.261321\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate8\",\r\n                            \"plate_Id\": \"-2033425075\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"9\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"5.92331\",\r\n                                        \"z\": \"5.361337\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Plate3\",\r\n                            \"plate_Id\": \"-2080479242\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"10\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-313\",\r\n                                        \"y\": \"2.793284\",\r\n                                        \"z\": \"1.246422\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"frameName\": \"Bosun Store Flat (23500 ABL) L53\",\r\n                    \"frame_Id\": \"-1399508946\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"Plate2\",\r\n                            \"plate_Id\": \"89495411\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"11\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-319.6454\",\r\n                                        \"y\": \"23.49999\",\r\n                                        \"z\": \"3.03626\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9999999\",\r\n                                        \"z\": \"-3.576279E-07\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"Stiffener33-1-1\",\r\n                            \"plate_Id\": \"-181258581\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"12\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 15.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-317.9652\",\r\n                                        \"y\": \"23.49999\",\r\n                                        \"z\": \"-0.9035492\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9999999\",\r\n                                        \"z\": \"-3.576279E-07\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}\r\n\r\n\r\n\r\n\r\n\r\n";

    // string data = "{\"id\":\"e9040407-2f03-47e2-840a-ee4b26650221\",\"gaugePlanName\":\"testfordemo\",\"description\":\"testfordemo\",\"imoNumber\":\"7654321\",\"eventId\":\"b5ff3ea3-bad9-440a-9d31-f6dd6cc23ef9\",\"compartments\":[{\"assetName\":\"CARGO TANK 03 S\",\"assetUID\":\"14016069\",\"frames\":[{\"id\":\"e9040407-3511-4dca-8970-8225e9c8ceed\",\"frameName\":\"ShipHull\",\"frame_Id\":\"-1998038115\",\"plates\":[{\"id\":\"e9040407-3511-4d27-b8aa-1b73cfcbf10b\",\"plateName\":\"Plate13\",\"plate_Id\":\"-1955484318\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4987-9fe1-7138d4561301\",\"point_Id\":\"3\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-167.7\",\"y\":\"18.40353\",\"z\":\"27.24965\"},\"normal\":{\"x\":\"9.213141E-08\",\"y\":\"4.115474E-05\",\"z\":\"-0.9999999\"}},{\"id\":\"e9040407-3511-42ca-9ace-72fc73528d89\",\"point_Id\":\"4\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-157.8109\",\"y\":\"18.18371\",\"z\":\"27.24964\"},\"normal\":{\"x\":\"9.213141E-08\",\"y\":\"4.115474E-05\",\"z\":\"-0.9999999\"}}]},{\"id\":\"e9040407-3511-49c7-ab40-a2f27848da1b\",\"plateName\":\"Plate297\",\"plate_Id\":\"-63372160\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4814-b852-9016316a4f11\",\"point_Id\":\"2\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12,\"measuredThickness\":11,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-164.9907\",\"y\":\"21.69147\",\"z\":\"27.24979\"},\"normal\":{\"x\":\"9.213141E-08\",\"y\":\"4.115474E-05\",\"z\":\"-0.9999999\"}},{\"id\":\"e9040407-3511-4bdb-bb8a-dda1e2b3a336\",\"point_Id\":\"1\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12,\"measuredThickness\":11,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-158.5489\",\"y\":\"21.61738\",\"z\":\"27.24978\"},\"normal\":{\"x\":\"9.213141E-08\",\"y\":\"4.115474E-05\",\"z\":\"-0.9999999\"}}]}]},{\"id\":\"e9040407-3511-463d-8838-f345457b91ee\",\"frameName\":\"FR 84 NT\",\"frame_Id\":\"414311966\",\"plates\":[{\"id\":\"e9040407-3511-43ad-8b36-395ced3048f8\",\"plateName\":\"Plate9\",\"plate_Id\":\"821075912\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-43d4-9776-48da1aab8812\",\"point_Id\":\"9\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":17,\"measuredThickness\":15,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"2.21522\",\"z\":\"17.41416\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4540-a5ee-a1bc06542a0e\",\"plateName\":\"Plate26\",\"plate_Id\":\"-735073657\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4373-a279-527a2a473ef2\",\"point_Id\":\"5\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":9,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"23.72484\",\"z\":\"25.43268\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4da6-9cb4-d3bb9b3da85b\",\"point_Id\":\"6\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":9,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"18.15715\",\"z\":\"25.87898\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4ba5-aedd-a502a041c1df\",\"plateName\":\"Plate30\",\"plate_Id\":\"831010278\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4570-bcde-31ffdb4046e7\",\"point_Id\":\"7\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":14,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"11.65492\",\"z\":\"25.97021\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-42e1-aaab-fb94d78f95d1\",\"point_Id\":\"8\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":15,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"9.121385\",\"z\":\"25.97109\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4bf7-b0b3-dcb06703fa8a\",\"plateName\":\"Plate22\",\"plate_Id\":\"-735073661\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-48ad-80a5-93bb1714c7d8\",\"point_Id\":\"11\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"11.01245\",\"z\":\"10.77148\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4bd3-9bf2-fdb700dbad4b\",\"point_Id\":\"10\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"7.145575\",\"z\":\"10.97285\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-485a-b4c4-f6616371928f\",\"plateName\":\"Plate24\",\"plate_Id\":\"-735073659\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4483-99a4-16fb55f33449\",\"point_Id\":\"12\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"17.1634\",\"z\":\"10.90769\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4c1d-9a16-7da0e64ea9e3\",\"point_Id\":\"13\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":12,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"21.85088\",\"z\":\"11.01149\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4c66-8cde-f7f7e0b86bc2\",\"plateName\":\"Plate8\",\"plate_Id\":\"-745008029\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-47c4-9c65-118df0d0a05f\",\"point_Id\":\"14\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"25.83056\",\"z\":\"16.13047\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4f42-9e46-8f12875fb09e\",\"point_Id\":\"15\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-194\",\"y\":\"26.14936\",\"z\":\"19.24765\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]}]},{\"id\":\"e9040407-3511-4ab7-b7f2-f58a24bf7223\",\"frameName\":\"FR 81 SWASH BHD\",\"frame_Id\":\"-1256822217\",\"plates\":[{\"id\":\"e9040407-3511-4ff1-9688-21b8efff5ec2\",\"plateName\":\"Plate6\",\"plate_Id\":\"-259312550\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4ac2-9cb5-6c950046c1f4\",\"point_Id\":\"16\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":10,\"lifeRemaining\":2.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"23.43464\",\"z\":\"25.52808\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4367-a922-94e32b734908\",\"point_Id\":\"17\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":10,\"lifeRemaining\":2.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"18.59389\",\"z\":\"25.85693\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4a0d-b2ed-3661ad5c3d41\",\"plateName\":\"Plate49\",\"plate_Id\":\"959906233\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-489f-bf83-93aa8916afa4\",\"point_Id\":\"20\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":15,\"lifeRemaining\":1,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"2.275273\",\"z\":\"21.85645\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-488f-87a0-f011eb3abeeb\",\"point_Id\":\"21\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":15,\"lifeRemaining\":1,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"2.050928\",\"z\":\"16.01002\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4fd8-9f7a-5bd26cb70aca\",\"plateName\":\"Plate46\",\"plate_Id\":\"-1056516402\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4596-956d-660059c64734\",\"point_Id\":\"22\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":1.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"6.759636\",\"z\":\"11.70168\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4a00-a164-ebad688a629c\",\"point_Id\":\"23\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":1.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"11.83084\",\"z\":\"11.2317\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-45da-be53-6ec77eeb1eea\",\"plateName\":\"Plate43\",\"plate_Id\":\"-1816031289\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-4d53-8769-483917369070\",\"point_Id\":\"18\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":1.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"12.0677\",\"z\":\"26.06905\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4f1c-81ac-8c028849679d\",\"point_Id\":\"19\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":12,\"lifeRemaining\":0.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"8.317419\",\"z\":\"25.99296\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4f30-92c2-83ea25a4bb97\",\"plateName\":\"Plate40\",\"plate_Id\":\"-249947348\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-48ab-91f8-0139f394aa35\",\"point_Id\":\"24\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":11,\"lifeRemaining\":1.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"17.7166\",\"z\":\"11.13723\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-4eb7-b7a2-af3b9ae28354\",\"point_Id\":\"25\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":10,\"lifeRemaining\":2.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"23.44615\",\"z\":\"10.93662\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040407-3511-4be6-801d-e38d2f800d39\",\"plateName\":\"Plate34\",\"plate_Id\":\"2075651475\",\"gaugingPoints\":[{\"id\":\"e9040407-3511-412e-8ea2-002fcac8df2d\",\"point_Id\":\"27\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":9,\"lifeRemaining\":3.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"26.05464\",\"z\":\"22.98357\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040407-3511-428b-b9dd-18d7724d7bd3\",\"point_Id\":\"26\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":10,\"lifeRemaining\":2.5,\"ruleThickness\":0,\"location\":{\"x\":\"-178.1\",\"y\":\"26.77185\",\"z\":\"14.72413\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]}]}]}]}";
    //string data_testFpso = "{\r\n    \"id\": \"e903110f-3532-42ad-93e1-a77b38189486\",\r\n    \"gaugePlanName\": \"test321\",\r\n    \"description\": \"test\",\r\n    \"imoNumber\": \"7370181\",\r\n    \"eventId\": \"6ed431c5-f511-4775-b351-172d619db2c6\",\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"Cargo Tank 03 S\",\r\n            \"assetUID\": \"14016069\",\r\n            \"frames\": [\r\n                {\r\n                    \"id\": \"e903120b-2204-4683-88b8-1d789a3cddd9\",\r\n                    \"frameName\": \"FR 85 NT\",\r\n                    \"frame_Id\": \"-1131568999\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"id\": \"e903120b-2204-487f-bffc-5a15021bde4a\",\r\n                            \"plateName\": \"Plate4\",\r\n                            \"plate_Id\": \"-2068327468\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4176-9981-34b24bbd0df0\",\r\n                                    \"point_Id\": \"25\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-199.3\",\r\n                                        \"y\": \"15.40497\",\r\n                                        \"z\": \"10.6696\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2204-43fe-9daa-aa9a8076396a\",\r\n                                    \"point_Id\": \"24\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-199.3\",\r\n                                        \"y\": \"20.18535\",\r\n                                        \"z\": \"10.87042\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-4d64-b720-93a43b903162\",\r\n                            \"plateName\": \"Plate6\",\r\n                            \"plate_Id\": \"-905528054\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-401a-a1ad-885da0c63c13\",\r\n                                    \"point_Id\": \"23\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-199.3\",\r\n                                        \"y\": \"17.82155\",\r\n                                        \"z\": \"25.55419\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4512-a6ed-be9132d4a8d6\",\r\n                                    \"point_Id\": \"22\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-199.3\",\r\n                                        \"y\": \"23.09172\",\r\n                                        \"z\": \"26.07437\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"id\": \"e903120b-2204-4d6a-9c36-2b24b243358c\",\r\n                    \"frameName\": \"L10 - LBHD Stbd\",\r\n                    \"frame_Id\": \"-437677687\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"id\": \"e903120b-2204-42d9-b4fb-9658c08299be\",\r\n                            \"plateName\": \"Plate115\",\r\n                            \"plate_Id\": \"1494160203\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-46a5-9dc3-6b29b9dfa653\",\r\n                                    \"point_Id\": \"12\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-199.8784\",\r\n                                        \"y\": \"9.735755\",\r\n                                        \"z\": \"9.399993\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"1\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-4d92-8d88-ac7edc899a2a\",\r\n                            \"plateName\": \"Plate116\",\r\n                            \"plate_Id\": \"1494160202\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-429d-a765-e8a1fc2571e1\",\r\n                                    \"point_Id\": \"19\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-190.0078\",\r\n                                        \"y\": \"4.401244\",\r\n                                        \"z\": \"9.399995\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-45e4-b97e-d216a0244d26\",\r\n                            \"plateName\": \"Plate113\",\r\n                            \"plate_Id\": \"1494160197\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4bc5-a702-0f3a871eec81\",\r\n                                    \"point_Id\": \"18\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-198.9799\",\r\n                                        \"y\": \"16.37418\",\r\n                                        \"z\": \"9.399991\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"1\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-49f7-ab35-e8da5a393cdd\",\r\n                            \"plateName\": \"Plate98\",\r\n                            \"plate_Id\": \"-1112432861\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4786-bbb4-521c1886dc2e\",\r\n                                    \"point_Id\": \"20\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-167.78\",\r\n                                        \"y\": \"14.91864\",\r\n                                        \"z\": \"9.399991\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"id\": \"e903120b-2204-4608-b90f-76336d92f803\",\r\n                    \"frameName\": \"Main Deck\",\r\n                    \"frame_Id\": \"-1143491428\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"id\": \"e903120b-2204-4336-8a94-814c32d9bcd9\",\r\n                            \"plateName\": \"Plate361\",\r\n                            \"plate_Id\": \"1736399843\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4c07-bd73-37420c193a25\",\r\n                                    \"point_Id\": \"13\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-164.8221\",\r\n                                        \"y\": \"27.39228\",\r\n                                        \"z\": \"18.95306\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9986338\",\r\n                                        \"z\": \"0.05225378\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4097-a744-e14a29815cbb\",\r\n                                    \"point_Id\": \"14\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-166.1445\",\r\n                                        \"y\": \"27.47064\",\r\n                                        \"z\": \"17.45549\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9986338\",\r\n                                        \"z\": \"0.05225378\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-4909-80d5-9d8a84c4e242\",\r\n                            \"plateName\": \"Plate221\",\r\n                            \"plate_Id\": \"1595237346\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4579-a3c0-14484a87daf1\",\r\n                                    \"point_Id\": \"16\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-187.9535\",\r\n                                        \"y\": \"27.44817\",\r\n                                        \"z\": \"17.88495\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9986338\",\r\n                                        \"z\": \"0.05225378\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4642-895c-db0f6f32d691\",\r\n                                    \"point_Id\": \"15\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-182.2193\",\r\n                                        \"y\": \"27.52538\",\r\n                                        \"z\": \"16.40934\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9986338\",\r\n                                        \"z\": \"0.05225378\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"id\": \"e903120b-2203-4407-8476-97fdd8be20ff\",\r\n                    \"frameName\": \"ShipHull\",\r\n                    \"frame_Id\": \"1177330163\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"id\": \"e903120b-2204-4987-9f93-6a7f45e51e2f\",\r\n                            \"plateName\": \"Plate249\",\r\n                            \"plate_Id\": \"569933127\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-46e9-968b-446ffb835ec9\",\r\n                                    \"point_Id\": \"21\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 24.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-207.4097\",\r\n                                        \"y\": \"1.69771\",\r\n                                        \"z\": \"26.86115\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1.362217E-06\",\r\n                                        \"y\": \"-0.4256562\",\r\n                                        \"z\": \"0.9048849\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-4ba7-b226-8bce917f105d\",\r\n                            \"plateName\": \"Plate491\",\r\n                            \"plate_Id\": \"1060677786\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4dfd-8c8c-9b890f71fff5\",\r\n                                    \"point_Id\": \"10\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-196.9496\",\r\n                                        \"y\": \"15.97563\",\r\n                                        \"z\": \"27.24999\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-4b87-9c2b-c810106acf47\",\r\n                            \"plateName\": \"Plate13\",\r\n                            \"plate_Id\": \"-1437872308\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-48fb-a411-3d06416a5337\",\r\n                                    \"point_Id\": \"7\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-170.4878\",\r\n                                        \"y\": \"17.91437\",\r\n                                        \"z\": \"27.24998\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"2.651257E-07\",\r\n                                        \"y\": \"-2.052352E-07\",\r\n                                        \"z\": \"1\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-4205-a1ab-d9042771e7c1\",\r\n                            \"plateName\": \"Plate466\",\r\n                            \"plate_Id\": \"-1311975224\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4cc1-acee-757a7ebb1a25\",\r\n                                    \"point_Id\": \"9\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-180.8717\",\r\n                                        \"y\": \"12.10815\",\r\n                                        \"z\": \"27.24999\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4a5f-9458-e3fa1ea8871d\",\r\n                                    \"point_Id\": \"8\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-176.9245\",\r\n                                        \"y\": \"12.55462\",\r\n                                        \"z\": \"27.24999\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-4e46-a736-d9ecfe57a0b0\",\r\n                            \"plateName\": \"Plate21\",\r\n                            \"plate_Id\": \"-1841156833\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4d25-8c42-390dc47fc8e8\",\r\n                                    \"point_Id\": \"11\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-192.7042\",\r\n                                        \"y\": \"19.40924\",\r\n                                        \"z\": \"27.24998\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"3.576279E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2204-4ec7-b7d4-fbfde18401d0\",\r\n                            \"plateName\": \"Plate415\",\r\n                            \"plate_Id\": \"-908690690\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2204-4263-9a74-7daa13dce5b6\",\r\n                                    \"point_Id\": \"17\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-192.0765\",\r\n                                        \"y\": \"25.58535\",\r\n                                        \"z\": \"27.24998\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"3.375657E-07\",\r\n                                        \"y\": \"-4.860534E-07\",\r\n                                        \"z\": \"0.9999999\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"id\": \"e903120b-2203-4c8f-a933-ede7bcaf06ef\",\r\n                    \"frameName\": \"FR 77 NT\",\r\n                    \"frame_Id\": \"1960204968\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"id\": \"e903120b-2203-46f7-b090-0c6cc6de5f3e\",\r\n                            \"plateName\": \"Plate19\",\r\n                            \"plate_Id\": \"467343079\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4658-999b-26866d4c4ea7\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"26.20367\",\r\n                                        \"z\": \"15.24824\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4199-b915-b7e11172d26d\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"25.84905\",\r\n                                        \"z\": \"22.13454\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-46aa-919b-2429fb206379\",\r\n                            \"plateName\": \"Plate35\",\r\n                            \"plate_Id\": \"-695456331\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-47c7-937e-6ec394218e74\",\r\n                                    \"point_Id\": \"5\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"1.652378\",\r\n                                        \"z\": \"15.18331\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4e03-907e-7587205fdd8b\",\r\n                                    \"point_Id\": \"6\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 17,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"1.481396\",\r\n                                        \"z\": \"19.54728\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"id\": \"e903120b-2203-4e87-9007-df88e4a6e2d1\",\r\n                            \"plateName\": \"Plate13\",\r\n                            \"plate_Id\": \"467343085\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"e903120b-2203-488d-8057-1730357bbe37\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"15.84454\",\r\n                                        \"z\": \"11.38739\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                },\r\n                                {\r\n                                    \"id\": \"e903120b-2203-4dba-b050-ec48f640e79e\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1,\r\n                                    \"lifeRemaining\": 0,\r\n                                    \"ruleThickness\": 0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-156.9\",\r\n                                        \"y\": \"21.65769\",\r\n                                        \"z\": \"9.634317\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}";

    public string data_testFpso = "{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"14016069\",\r\n            \"assetUID\": \"14016069\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"FR 83 NT\",\r\n                    \"frame_Id\": \"-1413894001\",\r\n                    \"frameImage\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"PLATE_398_7370181\",\r\n                            \"plate_Id\": \"Plate43\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"1\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-188.7\",\r\n                                        \"y\": \"16.51379\",\r\n                                        \"z\": \"22.11151\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"-1\",\r\n                                        \"y\": \"0\",\r\n                                        \"z\": \"0\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"BRACKET_1099_7370181\",\r\n                            \"plate_Id\": \"BKT142_1\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"2\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-189.0845\",\r\n                                        \"y\": \"17.55\",\r\n                                        \"z\": \"25.98906\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"-0.9999999\",\r\n                                        \"z\": \"3.576279E-07\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"STIFFENER_742_7370181\",\r\n                            \"plate_Id\": \"Stiffener8-1-1\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"3\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 16.0,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-188.7815\",\r\n                                        \"y\": \"22.02233\",\r\n                                        \"z\": \"25.38485\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.002910091\",\r\n                                        \"z\": \"-0.9999958\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"plateName\": \"STIFFENER_746_7370181\",\r\n                            \"plate_Id\": \"Stiffener56-1-1\",\r\n                            \"gaugingPoints\": [\r\n                                {\r\n                                    \"id\": \"\",\r\n                                    \"point_Id\": \"4\",\r\n                                    \"isClass\": true,\r\n                                    \"isRepresentative\": false,\r\n                                    \"originalThickness\": 12.5,\r\n                                    \"measuredThickness\": -1.0,\r\n                                    \"lifeRemaining\": 0.0,\r\n                                    \"ruledThickness\": 15.0,\r\n                                    \"location\": {\r\n                                        \"x\": \"-188.877\",\r\n                                        \"y\": \"19.25\",\r\n                                        \"z\": \"25.95653\"\r\n                                    },\r\n                                    \"normal\": {\r\n                                        \"x\": \"0\",\r\n                                        \"y\": \"0.9999999\",\r\n                                        \"z\": \"-3.576279E-07\"\r\n                                    }\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}\r\n\r\n";
    string data => usePetrobrasData ? data_petrobras : data_testFpso;
    // Stiffner Bracket 
    //string data_testFpso = "{\"id\":\"e9031f0b-3a34-48a0-859a-0af845bc34e0\",\"gaugePlanName\":\"tescz\",\"description\":\"Gauge Plan Name\",\"imoNumber\":\"7654321\",\"eventId\":\"b5ff3ea3-bad9-440a-9d31-f6dd6cc23ef9\",\"compartments\":[{\"assetName\":\"CARGO TANK 03 S\",\"assetUID\":\"14016069\",\"frames\":[{\"id\":\"e9040208-2520-4a2e-b7e9-d9338c3c4faf\",\"frameName\":\"FR 82 NT\",\"frame_Id\":\"1617517332\",\"plates\":[{\"id\":\"e9040208-2520-47e3-8430-21d851bc5983\",\"plateName\":\"BKT4_1\",\"plate_Id\":\"341845718\",\"gaugingPoints\":[{\"id\":\"e9040208-2520-4a0c-a54b-f1070decbd53\",\"point_Id\":\"7\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.8645\",\"y\":\"9.050004\",\"z\":\"25.902\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-49e7-a25a-7da16c38ad6e\",\"plateName\":\"Plate2\",\"plate_Id\":\"-924953969\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4ab2-a341-8617c51f3862\",\"point_Id\":\"14\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4\",\"y\":\"26.08326\",\"z\":\"17.06627\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040208-2521-4a00-939d-9f63629c04da\",\"point_Id\":\"13\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4\",\"y\":\"25.98888\",\"z\":\"23.11597\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2520-46cc-a865-8a82cf4a4dda\",\"plateName\":\"BKT173_1\",\"plate_Id\":\"1947077247\",\"gaugingPoints\":[{\"id\":\"e9040208-2520-4081-81ce-857a248883bb\",\"point_Id\":\"5\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.7653\",\"y\":\"10.75\",\"z\":\"26.01112\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}},{\"id\":\"e9040208-2520-410f-8274-ff92f3126748\",\"point_Id\":\"6\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4669\",\"y\":\"10.75\",\"z\":\"24.55519\"},\"normal\":{\"x\":\"0\",\"y\":\"-1\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2520-4a20-8aaa-beca39096f02\",\"plateName\":\"BKT154_1\",\"plate_Id\":\"-1084334152\",\"gaugingPoints\":[{\"id\":\"e9040208-2520-463e-879c-53bc956f7fe4\",\"point_Id\":\"2\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.8004\",\"y\":\"17.55\",\"z\":\"25.72063\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}},{\"id\":\"e9040208-2520-417a-b7e6-8ecda71d59a9\",\"point_Id\":\"1\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.5916\",\"y\":\"17.55\",\"z\":\"26.28017\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-4431-9395-d30cf059f42a\",\"plateName\":\"Plate18\",\"plate_Id\":\"294833986\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4986-9bdb-caa68061a241\",\"point_Id\":\"17\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4\",\"y\":\"24.20048\",\"z\":\"10.38952\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2520-4232-948a-d99b7b4df2af\",\"plateName\":\"Stiffener201-1-1\",\"plate_Id\":\"-1202317370\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-44c1-9fb9-a27faee253b6\",\"point_Id\":\"11\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":15,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.5688\",\"y\":\"7.453201\",\"z\":\"11.26635\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.000623205\",\"z\":\"-0.9999998\"}},{\"id\":\"e9040208-2521-40d2-937c-f618de45186c\",\"point_Id\":\"12\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":15,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4794\",\"y\":\"13.29096\",\"z\":\"11.26271\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.000623205\",\"z\":\"-0.9999998\"}}]},{\"id\":\"e9040208-2520-4267-b8c2-ea5c45ba36a2\",\"plateName\":\"Stiffener198-1-1\",\"plate_Id\":\"656692431\",\"gaugingPoints\":[{\"id\":\"e9040208-2520-48a2-b5fb-860385ed3e22\",\"point_Id\":\"10\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":15,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.5084\",\"y\":\"12.54963\",\"z\":\"25.35728\"},\"normal\":{\"x\":\"0\",\"y\":\"0.002910091\",\"z\":\"-0.9999958\"}},{\"id\":\"e9040208-2520-4bb6-8ad9-abdfb7367141\",\"point_Id\":\"9\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":15,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.5137\",\"y\":\"22.02266\",\"z\":\"25.38485\"},\"normal\":{\"x\":\"0\",\"y\":\"0.002910091\",\"z\":\"-0.9999958\"}},{\"id\":\"e9040208-2520-48ac-979e-e507b2d8c56c\",\"point_Id\":\"8\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":15,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4599\",\"y\":\"23.6734\",\"z\":\"25.38966\"},\"normal\":{\"x\":\"0\",\"y\":\"0.002910091\",\"z\":\"-0.9999958\"}}]},{\"id\":\"e9040208-2521-4eee-b376-fbb8f5ba7f02\",\"plateName\":\"Plate3\",\"plate_Id\":\"641129972\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-482f-92a1-0706214caf27\",\"point_Id\":\"15\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":17,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4\",\"y\":\"1.518785\",\"z\":\"18.86249\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040208-2521-4457-a642-2ce13a6177f9\",\"point_Id\":\"16\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":17,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.4\",\"y\":\"1.191895\",\"z\":\"13.61751\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2520-4059-a08a-ff9a60d1903d\",\"plateName\":\"BKT75_1\",\"plate_Id\":\"2012679522\",\"gaugingPoints\":[{\"id\":\"e9040208-2520-44c2-bc57-ab3daff0b19f\",\"point_Id\":\"3\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-184.1399\",\"y\":\"15.85\",\"z\":\"26.48877\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}},{\"id\":\"e9040208-2520-43a9-9519-c13d8521c4fd\",\"point_Id\":\"4\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-183.6839\",\"y\":\"15.85\",\"z\":\"25.16016\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]}]},{\"id\":\"e9040208-2521-4385-a413-f78f9472ca18\",\"frameName\":\"FR 83 NT\",\"frame_Id\":\"-1413894001\",\"plates\":[{\"id\":\"e9040208-2521-488a-bfb3-04b754bd7594\",\"plateName\":\"Plate27\",\"plate_Id\":\"582925737\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4914-b654-e50c29c3ad39\",\"point_Id\":\"28\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":16,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.7\",\"y\":\"2.087793\",\"z\":\"24.9175\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2521-4f32-8064-1f4c07245b1a\",\"plateName\":\"Plate2\",\"plate_Id\":\"2092021140\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4a79-94a7-0d09cc7317b5\",\"point_Id\":\"25\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.7\",\"y\":\"26.11892\",\"z\":\"16.98479\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040208-2521-4db3-abc5-7770aa18eef0\",\"point_Id\":\"24\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.7\",\"y\":\"26.14623\",\"z\":\"22.8894\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2521-49d2-a913-2194f5cdceb1\",\"plateName\":\"BKT142_1\",\"plate_Id\":\"-1224093124\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4632-baeb-7ba81719682a\",\"point_Id\":\"18\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.9991\",\"y\":\"17.55\",\"z\":\"25.72165\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-482f-92fb-3880481a3041\",\"plateName\":\"BKT146_1\",\"plate_Id\":\"-1788743128\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-46a4-8c15-a67b136abc42\",\"point_Id\":\"20\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.9955\",\"y\":\"10.75\",\"z\":\"25.66614\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-4e06-acc6-612c20a3516c\",\"plateName\":\"BKT31_1\",\"plate_Id\":\"1822304837\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-43a0-8cf5-d8b6b270d991\",\"point_Id\":\"19\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-189.0225\",\"y\":\"15.85\",\"z\":\"25.80672\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-4e79-9441-9046f18957e8\",\"plateName\":\"BKT28_1\",\"plate_Id\":\"518343153\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-48b0-981d-62e12e0aeaae\",\"point_Id\":\"22\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.8192\",\"y\":\"9.049998\",\"z\":\"10.81069\"},\"normal\":{\"x\":\"0\",\"y\":\"0.9999999\",\"z\":\"-3.576279E-07\"}}]},{\"id\":\"e9040208-2521-430b-92dd-abd2c60dc8a6\",\"plateName\":\"Plate3\",\"plate_Id\":\"525937199\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4326-8f10-1569a50610a8\",\"point_Id\":\"26\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":17,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.7\",\"y\":\"1.560488\",\"z\":\"13.35954\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}},{\"id\":\"e9040208-2521-4e8c-bc8a-7c2d0a0c9227\",\"point_Id\":\"27\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":17,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.7\",\"y\":\"1.562786\",\"z\":\"18.548\"},\"normal\":{\"x\":\"-1\",\"y\":\"0\",\"z\":\"0\"}}]},{\"id\":\"e9040208-2521-4740-bba7-d223b0664d3b\",\"plateName\":\"BKT171_1\",\"plate_Id\":\"-800605586\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4d08-832d-01ff8f717ffe\",\"point_Id\":\"21\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-189.0553\",\"y\":\"9.050004\",\"z\":\"25.72998\"},\"normal\":{\"x\":\"0\",\"y\":\"-0.9999999\",\"z\":\"3.576279E-07\"}}]},{\"id\":\"e9040208-2521-461b-a54a-efad6020da7f\",\"plateName\":\"BKT199_1\",\"plate_Id\":\"328694480\",\"gaugingPoints\":[{\"id\":\"e9040208-2521-4fd9-b8e1-e7efb79e41eb\",\"point_Id\":\"23\",\"isClass\":true,\"isRepresentative\":false,\"originalThickness\":12.5,\"measuredThickness\":-1,\"lifeRemaining\":0,\"ruleThickness\":0,\"location\":{\"x\":\"-188.9483\",\"y\":\"15.85\",\"z\":\"11.03109\"},\"normal\":{\"x\":\"0\",\"y\":\"0.9999999\",\"z\":\"-3.576279E-07\"}}]}]}]}]}";

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera screenshotCamera;
    [SerializeField] private string savePath = "D:/Screenshot/";
    [SerializeField] private TextMeshProUGUI frameText;
    [SerializeField] private TextMeshProUGUI compartmentText;
    [SerializeField] private TextMeshProUGUI SubtypeText;
    [SerializeField] private RectTransform baseCanvas;
    [SerializeField] private VirtualizedGrid virtualizedGrid;
    [SerializeField] private Material bracket_Mat;
    [SerializeField] private Material stiffner_Mat;
    [SerializeField] private GameObject cameraBG;
    [SerializeField] private float frameLabelPadding = 4f;
    [SerializeField] private Vector3 boundsOffset;
    [SerializeField] private float labelSize = 1f;
    [SerializeField] private bool usePetrobrasData = false;
    [SerializeField] private GameObject dottedMarker;
    [SerializeField][Range(0.5f, 3f)] private float imageQuality;

    private void Start()
    {
        // 1. Temp - to test screenshot localy
#if UNITY_EDITOR
        System.IO.Directory.CreateDirectory(savePath);
        frameText.text = "No Object Selected";
        compartmentText.text = "No Object Selected";
#endif
        EventBroadcaster.OnCompartmentLoadingStarted += OnCompartmentLoadStarted;
        EventBroadcaster.OnCompartmentLoadingComplete += OnCompartmentLoaded;
    }

    bool compartmentLoading = false;
    private void OnCompartmentLoadStarted(string obj)
    {
        compartmentLoading = true;
    }
    private void OnCompartmentLoaded(string compartmentName)
    {
        compartmentLoading = false;
    }

    public override void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
#if UNITY_EDITOR
            //string path = EditorUtility.OpenFilePanel("Gauge Plan", Application.dataPath, "json");
            //string jsonData = System.IO.File.ReadAllText(path);
            CaptureHullPartScreenshot(/*jsonData*/);
#endif
        }
    }

    // Receive Hull Part Name & Capture Screenshot
    public void CaptureHullPartScreenshot()
    {
        //GaugePointsData_External deserializedData = JsonUtility.FromJson<GaugePointsData_External>(data);
        //GaugingManager.Instance.ClearGaugePoints();
        //GaugingManager.Instance.SpawnLoadedGaugePoints(deserializedData);

        var frameNames = GaugingManager.Instance.GetDeserializedData().compartments.SelectMany(c => c.frames.Select(f => $"{c.assetUID}/{f.frameName}")).ToList();

        CommunicationManager.Instance.WireframeMode_Extern();

        CommunicationManager.Instance.ShowPlates_Extern();
        CommunicationManager.Instance.ShowBrackets_Extern();
        CommunicationManager.Instance.ShowStiffeners_Extern();

        OutlinePassFinal.inScreenshotMode = true;

        StartCoroutine(CaptureScreenshot(0, frameNames, GaugingManager.Instance.GetDeserializedData()));
    }

    private List<GameObject> dottedLineMarkers = new List<GameObject>();

    // Capture Screenshot
    private IEnumerator CaptureScreenshot(int index, List<string> framePaths, GaugePointsData_External deserializedData)
    {
        foreach (var item in dottedLineMarkers)
        {
            Destroy(item);
        }

        //dottedLineMarkers.Clear();

        if (index >= framePaths.Count)
        {
            CommunicationManager.Instance.OpaqueMode_Extern("1");
          //  GaugingManager.Instance.ClearGaugePoints();
            CameraService.Instance.cameraTransform = mainCamera.transform;
            string jsonData = JsonUtility.ToJson(deserializedData, true);
            CommunicationManager.Instance.HandleCapturedImagesForExport_Extern(jsonData);

            //ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(false);

            mainCamera.transform.GetChild(0).gameObject.SetActive(true);
            cameraBG.SetActive(true);
           // ApplicationStateMachine.Instance.ResetStateMachine();
            OutlinePassFinal.inScreenshotMode = false;

            yield break;
        }

        mainCamera.transform.GetChild(0).gameObject.SetActive(false);
        cameraBG.SetActive(false);
        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(true, "Capturing Screenshot ...");
        CameraService.Instance.cameraTransform = screenshotCamera.transform;

        var breadCrums = framePaths[index].Split('/');
        var compartmentName = breadCrums[0];
        var hullpartName = breadCrums[1];

        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { compartmentName });
        var compartment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName);

        while (compartmentLoading)
        {
            yield return null;
        }

        compartmentText.text = "<b><color=black>Compartment : </color>" + GroupingManager.Instance.vesselObject.GetCompartmentName(compartmentName) + "</b>";
        frameText.text = "<b><color=black>Hullpart : </b></color>" + hullpartName + "</b>";

        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
        {
            c.SetActive(c.uid == compartmentName);
        });

        GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, h => h).ForEach(h =>
        {
            h.SetActiveByType(h.name == hullpartName, SubpartType.Plate);
            h.SetActiveByType(h.name == hullpartName, SubpartType.Bracket);
            h.SetActiveByType(h.name == hullpartName, SubpartType.Stiffener);
        });

        Hullpart hullpartReference = GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName);

        List<MeshFilter> brackets = hullpartReference.GetSubparts(SubpartType.Bracket, s => s.subpartMeshRenderer.GetComponent<MeshFilter>());
        List<MeshFilter> stiffeners = hullpartReference.GetSubparts(SubpartType.Stiffener, s => s.subpartMeshRenderer.GetComponent<MeshFilter>());
        List<Renderer> plates = hullpartReference.GetSubparts(SubpartType.Plate, s => s.subpartMeshRenderer.GetComponent<Renderer>());

        MeshExtruder.Instance.Extrude(brackets, 60f, bracket_Mat);
        MeshExtruder.Instance.Extrude(stiffeners, 40f, stiffner_Mat);

        GaugingManager.Instance.SetAllGaugePointsActive(false);
        List<KeyValuePair<GameObject, GaugePointData>> points = GaugingManager.Instance.GetGaugePoints(compartmentName, hullpartName);

        //GaugingManager.Instance.IsolateGaugePoints(compartmentName, hullpartName, out List<KeyValuePair<GameObject, GaugePointData>> points);
        ViewDirection viewDirection = PositionCamera(plates, out Bounds combinedBounds);
        var transverseLabels = ServiceRunner.GetService<FrameLabelFeature>().GetTransverseLabelsOfCompartment(compartment);
        var longitudinalLabels = ServiceRunner.GetService<FrameLabelFeature>().GetLongitudinalLabelsOfCompartment(compartment);

        if (viewDirection != ViewDirection.Front)
        {
            foreach (var item in transverseLabels)
            {
                Hullpart labelingHullpart = compartment.GetHullpart(item.gameObject.name);
                Vector3 transPosition = TransverseFrameLabelManager.GetPosition(item, labelingHullpart);
                GameObject g = Instantiate(dottedMarker);
                FrameLabel label = g.transform.GetChild(0).GetComponent<FrameLabel>();
                dottedLineMarkers.Add(g.gameObject);
                Vector3 pos = combinedBounds.center;
                pos.x = transPosition.x;
                g.transform.position = pos;

                Quaternion rotation = Quaternion.identity;
                float zSize = 0f;
                switch (viewDirection)
                {
                    case ViewDirection.Default:
                        rotation = Quaternion.identity;
                        break;
                    case ViewDirection.Front:
                        break;
                    case ViewDirection.Top:
                        rotation = Quaternion.identity;
                        zSize = (combinedBounds.max.z - combinedBounds.min.z) + frameLabelPadding;
                        break;
                    case ViewDirection.Side:
                        rotation = Quaternion.LookRotation(-Vector3.up, Vector3.forward);
                        zSize = (combinedBounds.max.y - combinedBounds.min.y) + frameLabelPadding;
                        break;
                    default:
                        break;
                }

                g.transform.rotation = rotation;
                label.size = zSize;
                label.UpdateLRS(screenshotCamera.transform, item.label);
            }
        }

        if (viewDirection != ViewDirection.Side)
        {
            foreach (var item in longitudinalLabels)
            {
                Hullpart labelingHullpart = compartment.GetHullpart(item.gameObject.name);
                Vector3 longPosition = LongitudinalFrameLabelManager.GetPosition(item, labelingHullpart);
                GameObject g = Instantiate(dottedMarker);
                FrameLabel label = g.transform.GetChild(0).GetComponent<FrameLabel>();
                dottedLineMarkers.Add(g.gameObject);
                Vector3 pos = combinedBounds.center;
                pos.z = longPosition.z;
                g.transform.position = pos;

                Quaternion rotation = Quaternion.identity;
                float xSize = 0f;
                switch (viewDirection)
                {
                    case ViewDirection.Default:
                        rotation = Quaternion.identity;
                        break;
                    case ViewDirection.Front:
                        rotation = Quaternion.LookRotation(-Vector3.up, -Vector3.right);
                        xSize = (combinedBounds.max.y - combinedBounds.min.y) + frameLabelPadding;
                        break;
                    case ViewDirection.Top:
                        rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                        xSize = (combinedBounds.max.x - combinedBounds.min.x) + frameLabelPadding;
                        break;
                    case ViewDirection.Side:
                        break;
                    default:
                        break;
                }

                g.transform.rotation = rotation;
                label.size = xSize;
                label.UpdateLRS(screenshotCamera.transform, item.label);
            }
        }

        //Debug.Log($"From {combinedBounds.min.z} || To {combinedBounds.max.z}");
        //PositionCamera(GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName));
        // yield return new WaitForEndOfFrame();

        GaugingPointIndicator.LookAt(screenshotCamera);

        //----------Canvas UI element------------------------------------
        //ObjectLabeler.Instance.SpawnGaugePointUI(points);
        //ObjectLabeler.Instance.CreateLabelsForAllObjects(GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName));
        //-------------------------------------------------------

        Vector2 dimensions = GetTextureResolution(screenshotCamera, combinedBounds, Mathf.FloorToInt(1280 * imageQuality), Mathf.FloorToInt(1280 * imageQuality));
        //float multiplier_Label = 4f / ((1561f * 4f) / (dimensions.x));

        baseCanvas.sizeDelta = GetRequiredCanvasSize(screenshotCamera, combinedBounds) * 2f;
        virtualizedGrid.gridDimensions = baseCanvas.sizeDelta * dimensionsMultiplier;
        baseCanvas.transform.position = combinedBounds.center;
        virtualizedGrid.transform.position = baseCanvas.transform.position;
        baseCanvas.transform.rotation = Quaternion.LookRotation(screenshotCamera.transform.forward, screenshotCamera.transform.up);
        virtualizedGrid.transform.rotation = baseCanvas.transform.rotation;

        Vector3 offset = virtualizedGrid.transform.up * (virtualizedGrid.gridDimensions.y);
        offset += virtualizedGrid.transform.right * (virtualizedGrid.gridDimensions.x);

        virtualizedGrid.transform.localPosition -= (offset / 2f);
        virtualizedGrid.GenerateGrid();

        ObjectLabeler3D.Instance.Create3DLabelsForAllObjects(GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName), labelSize, virtualizedGrid.transform);

        //ObjectLabeler3D.Instance.SpawnGaugePointUI_3D(points);
        //--------Render Image--------------------------
        //string base64String = RenderImage(hullpartName, combinedBounds);
        //--------Sending back String image to json------------

        SubpartType[] typesToProcess = { SubpartType.Plate, SubpartType.Bracket, SubpartType.Stiffener };

        List<string> base64Images = new();
        foreach (var type in typesToProcess)
        {
            if (ObjectLabeler3D.Instance.SpawnGaugePointUI_3D(points, type, virtualizedGrid.transform))
            {
                ObjectLabeler3D.Instance.UpdateLabelPositions_3D();
                virtualizedGrid.UpdatePositionsInGrid();
                Canvas.ForceUpdateCanvases();
                //if(type == SubpartType.Bracket)
                //{
                //    ObjectLabeler3D.Instance.RelaxLabelsScreenSpace2D(screenshotCamera);
                //}
                yield return null;
                SubtypeText.text = "<b><color=black>Gaugepoints on : </b></color>" + type + "</b>";
                string base64String = RenderImage(hullpartName, combinedBounds, type);
                string labeledBase64 = $"-//{hullpartName}-{type}//-{base64String}"; // Final format
                base64Images.Add(labeledBase64);
                ObjectLabeler3D.Instance.ClearGaugePointLabels();
                yield return null;
            }
        }

        string combinedBase64 = string.Join("--///--", base64Images);

        deserializedData.compartments.Where(c => c.assetUID == compartmentName).FirstOrDefault().
        frames.Where(f => f.frameName == hullpartName).FirstOrDefault().frameImage = combinedBase64;



        ////--------Render Multiple Images---------------------

        //string[] views = new string[] { "FWD", "TOP", "STBD" };
        //Dictionary<string, string> base64Images = new Dictionary<string, string>();

        //foreach (string view in views)
        //{
        //    // Set camera
        //    PositionCameraView(plates, view);
        //    // Make sure indicator faces camera
        //    GaugingPointIndicator.LookAt(screenshotCamera);

        //    // Create labels
        //    ObjectLabeler3D.Instance.SpawnGaugePointUI_3D(points);
        //    ObjectLabeler3D.Instance.Create3DLabelsForAllObjects(GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName));

        //    // Render and store image
        //    string base64String1 = RenderImage(hullpartName, view);
        //    base64Images[view] = base64String1;

        //    yield return new WaitForSeconds(0.1f);
        //}

        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
        {
            if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
            {
                string targettedCompartmentName = (ApplicationStateMachine.Instance.CurrentState as CompartmentViewState).GetTargettedCompartemnt();
                c.SetActive(c.uid == targettedCompartmentName);
            }
            else
            {
                c.SetActive(true);
            }
        });

        GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, h => h).ForEach(h =>
        {
            h.SetActive(true);
            if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
            {
                h.GetCollider().enabled = false;
            }
        });

        //ObjectLabeler.Instance.ClearLabels();

        ObjectLabeler3D.Instance.ClearGeneralLabels();
        ObjectLabeler3D.Instance.ClearGaugePointLabels();
        GaugingPointIndicator.ResetLookAtAll();
        MeshExtruder.Instance.Revert();
        plates.Clear();

        yield return null;

        index++;
        StartCoroutine(CaptureScreenshot(index, framePaths, deserializedData));
    }


    /// <summary>
    /// Position camera based on Active Plates  (No need of hullpart) 
    /// </summary>
    /// <param name="plates"></param>

    private ViewDirection PositionCamera(List<Renderer> plates, out Bounds combinedBounds)
    {
        ViewDirection viewDirection = ViewDirection.Default;

        combinedBounds = new Bounds();
        if (plates == null || plates.Count == 0)
        {
            Debug.LogError("PositionCamera: No renderers provided.");
            return viewDirection;
        }

        // Start with null bounds and find the first valid one
        combinedBounds = new Bounds();
        bool boundsInitialized = false;

        foreach (Renderer r in plates)
        {
            if (r != null && r.bounds.size != Vector3.zero)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = r.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }
        }

        if (!boundsInitialized)
        {
            Debug.LogError("PositionCamera: No valid renderers with bounds.");
            return viewDirection;
        }

        Vector3 size = combinedBounds.size;
        Vector3 center = combinedBounds.center;

        // Calculate surface areas to decide view direction
        float areaXY = size.x * size.y;
        float areaXZ = size.x * size.z;
        float areaYZ = size.y * size.z;

        Vector3 normal;
        Vector3 offset;
        if (areaXY >= areaXZ && areaXY >= areaYZ)
        {
            normal = Vector3.forward;  // Front view
            viewDirection = ViewDirection.Side;
            offset = Vector3.up * 2f;
            combinedBounds.size = new Vector3(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
        }
        else if (areaXZ >= areaXY && areaXZ >= areaYZ)
        {
            normal = Vector3.up;       // Top view
            viewDirection = ViewDirection.Top;
            offset = Vector3.forward * -2f;
            combinedBounds.size = new Vector3(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
        }
        else
        {
            normal = Vector3.right;    // Side view
            viewDirection = ViewDirection.Front;
            offset = Vector3.up * 2f;
            combinedBounds.size = new Vector3(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
        }

        float distance = Mathf.Max(size.x, size.y, size.z) * 2.5f;
        Vector3 cameraPos = center - (normal * distance);
        combinedBounds.size += boundsOffset;
        FrameObject(combinedBounds, screenshotCamera, center - cameraPos, offset * 0f);
        return viewDirection;
    }

    public void FrameObject(
     Bounds bounds,
     Camera screenshotCamera,
     Vector3 normal,
     Vector3 offset)
    {
        Vector3 size = bounds.size;
        Vector3 center = bounds.center;
        Vector3 direction = normal.normalized;

        // Position and orient the camera
        float distance = size.magnitude;
        Vector3 cameraPos = center - direction * distance;
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        screenshotCamera.transform.position = cameraPos + offset;
        screenshotCamera.transform.rotation = lookRotation;

        // Compute object corners
        Vector3[] corners = new Vector3[8];
        Vector3 extents = bounds.extents;
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    corners[i++] = center + Vector3.Scale(new Vector3(x, y, z), extents);
                }
            }
        }

        // Measure bounds in camera local space
        float maxX = 0f, maxY = 0f;
        foreach (Vector3 corner in corners)
        {
            Vector3 local = screenshotCamera.transform.InverseTransformPoint(corner);
            maxX = Mathf.Max(maxX, Mathf.Abs(local.x));
            maxY = Mathf.Max(maxY, Mathf.Abs(local.y));
        }

        float objectWidth = maxX * 2f;
        float objectHeight = maxY * 2f;

        float aspect = GetRequiredAspectRatio(screenshotCamera, bounds, out _);

        // Apply uniform padding to both width and height based on dominant size
        float paddedWidth = objectWidth;
        float paddedHeight = objectHeight;

        float orthoSizeByHeight = paddedHeight / 2f;
        float orthoSizeByWidth = (paddedWidth / aspect) / 2f;
        float finalOrthoSize = Mathf.Max(orthoSizeByHeight, orthoSizeByWidth);

        screenshotCamera.orthographic = true;
        screenshotCamera.orthographicSize = finalOrthoSize;
    }


    /// <summary>
    /// Position Camera Old method if hulppart is present
    /// </summary>
    /// <param name="targetObject"></param>
    private void PositionCamera(Hullpart targetObject)
    {

        Renderer[] renderers = targetObject.hullpartMeshReference.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogError("PositionCamera: No active renderers found for Hullpart: " + targetObject.name);
            return;
        }

        // Calculate bounding box
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 size = bounds.size;
        Vector3 center = bounds.center;

        // Calculate surface areas
        float areaXY = size.x * size.y;
        float areaXZ = size.x * size.z;
        float areaYZ = size.y * size.z;

        // Choose view direction based on largest side
        Vector3 normal;
        if (areaXY >= areaXZ && areaXY >= areaYZ)
            normal = Vector3.forward;  // Front view
        else if (areaXZ >= areaXY && areaXZ >= areaYZ)
            normal = Vector3.up;       // Top view
        else
            normal = Vector3.right;    // Side view

        float distance = Mathf.Max(size.x, size.y, size.z) * 2.5f;
        Vector3 cameraPos = center - (normal * distance);

        screenshotCamera.transform.position = cameraPos;
        screenshotCamera.transform.rotation = Quaternion.LookRotation(center - cameraPos, Vector3.up);

        if (frameText != null)
            frameText.text = "Hullpart: " + targetObject.name;
    }



    /// <summary>
    /// Position Camera Multiple Views
    /// </summary>
    /// <param name="targetObject"></param>
    /// <param name="view"></param>
    private void PositionCameraView(List<Renderer> plates, String view)
    {

        if (plates == null || plates.Count == 0)
        {
            Debug.LogError("PositionCamera: No renderers provided.");
            return;
        }

        // Start with null bounds and find the first valid one
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;

        foreach (Renderer r in plates)
        {
            if (r != null && r.bounds.size != Vector3.zero)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = r.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }
        }

        if (!boundsInitialized)
        {
            Debug.LogError("PositionCamera: No valid renderers with bounds.");
            return;
        }

        //Vector3 size = combinedBounds.size;
        Vector3 center = combinedBounds.center;

        float objectSize = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
        float distance = objectSize * 2.5f;

        screenshotCamera.transform.LookAt(center);

        switch (view)
        {
            case "FWD":
                screenshotCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;

            case "BCK":
                screenshotCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
                break;

            case "TOP":
                screenshotCamera.transform.rotation = Quaternion.Euler(90, 0, 180);
                break;

            case "BTM":
                screenshotCamera.transform.rotation = Quaternion.Euler(-90, 0, 0);
                break;

            case "STBD":
                screenshotCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;

            case "PORT":
                screenshotCamera.transform.rotation = Quaternion.Euler(0, -180, 0);
                break;

            default:
                Debug.LogWarning($"View '{view}' not recognized. No rotation applied.");
                break;
        }

        screenshotCamera.transform.position = center - (screenshotCamera.transform.forward * distance);

    }

    float GetRequiredAspectRatio(Camera cam, Bounds b, out bool isWidthDominant, bool useRawAspect = true)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 center = b.center;
        Vector3 extents = b.extents;
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    corners[i++] = center + Vector3.Scale(new Vector3(x, y, z), extents);
                }
            }
        }

        float maxX = 0f, maxY = 0f;
        foreach (Vector3 corner in corners)
        {
            Vector3 local = cam.transform.InverseTransformPoint(corner);
            maxX = Mathf.Max(maxX, Mathf.Abs(local.x));
            maxY = Mathf.Max(maxY, Mathf.Abs(local.y));
        }

        isWidthDominant = maxX > maxY;

        if (useRawAspect)
        {
            return (maxX * 2f) / (maxY * 2f); // width / height
        }
        else
        {
            if (isWidthDominant)
            {
                return maxY / maxX; // height / width
            }
            else
            {
                return maxX / maxY; // width / height
            }
        }
    }

    Vector2 GetRequiredCanvasSize(Camera cam, Bounds b)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 center = b.center;
        Vector3 extents = b.extents;
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    corners[i++] = center + Vector3.Scale(new Vector3(x, y, z), extents);
                }
            }
        }

        float maxX = 0f, maxY = 0f;
        foreach (Vector3 corner in corners)
        {
            Vector3 local = cam.transform.InverseTransformPoint(corner);
            maxX = Mathf.Max(maxX, Mathf.Abs(local.x));
            maxY = Mathf.Max(maxY, Mathf.Abs(local.y));
        }

        return new Vector2(maxX, maxY);
    }

    public Vector2 GetTextureResolution(Camera screenshotCamera, Bounds bounds, int baseResolutionWidth = 1920, int baseResolutionHeight = 1080)
    {
        float aspectRatio = GetRequiredAspectRatio(screenshotCamera, bounds, out bool isWidthDominant, false);
        int width = baseResolutionWidth;
        int height = baseResolutionHeight;

        // Step 2: Compute width/height based on object aspect ratio
        if (isWidthDominant)
        {
            height = Mathf.CeilToInt(baseResolutionWidth * aspectRatio);
        }
        else
        {
            width = Mathf.CeilToInt(baseResolutionHeight * aspectRatio);
        }
        return new Vector2(width, height);
    }

    public Texture2D CaptureObjectSmartScreenshot(Camera screenshotCamera, Bounds bounds, int baseResolutionWidth = 1920, int baseResolutionHeight = 1080)
    {
        // Step 1: Calculate required aspect ratio based on camera-relative object size
        //float aspectRatio = GetRequiredAspectRatio(screenshotCamera, bounds, out bool isWidthDominant, false);


        //// Step 2: Compute width/height based on object aspect ratio
        //if(isWidthDominant)
        //{
        //    height = Mathf.CeilToInt(baseResolutionWidth * aspectRatio);
        //}
        //else
        //{
        //    width = Mathf.CeilToInt(baseResolutionHeight * aspectRatio);
        //}
        Vector2 dimensions = GetTextureResolution(screenshotCamera, bounds, baseResolutionWidth, baseResolutionHeight);
        int width = Mathf.CeilToInt(dimensions.x);
        int height = Mathf.CeilToInt(dimensions.y);

        // Step 3: Create RenderTexture and render
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rt.Create();

        screenshotCamera.targetTexture = rt;
        RenderTexture.active = rt;
        screenshotCamera.Render();

        // Step 4: Read pixels into Texture2D
        Texture2D sceneScreenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        sceneScreenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        sceneScreenshot.Apply();

        // Step 5: Cleanup
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);

        return sceneScreenshot;
    }

    public string RenderImage(string hullpartName, Bounds bounds)
    {
        Texture2D sceneScreenshot = CaptureObjectSmartScreenshot(screenshotCamera, bounds, Mathf.FloorToInt(1280 * imageQuality), Mathf.FloorToInt(1280 * imageQuality));

#if UNITY_EDITOR
        string filePath = Path.Combine(savePath, $"{hullpartName}_Screenshot.png");
        byte[] bytes = sceneScreenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
#endif

        string base64String = Convert.ToBase64String(sceneScreenshot.EncodeToPNG());

        Destroy(sceneScreenshot);
        return base64String;
    }

    public string RenderImage(string hullpartName, Bounds bounds, SubpartType type)
    {
        Texture2D sceneScreenshot = CaptureObjectSmartScreenshot(
            screenshotCamera,
            bounds,
            Mathf.FloorToInt(1280 * imageQuality),
            Mathf.FloorToInt(1280 * imageQuality)
        );

#if UNITY_EDITOR
        // Create a more specific filename using the subpart type
        string typeName = type.ToString().ToLower(); // "plate", "bracket", etc.
        string filePath = Path.Combine(savePath, $"{hullpartName}_Screenshot_{typeName}.png");

        byte[] bytes = sceneScreenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
#endif

        string base64String = Convert.ToBase64String(sceneScreenshot.EncodeToPNG());

        Destroy(sceneScreenshot);
        return base64String;
    }

    public string RenderImage(string hullpartName, string view)
    {
        int width = Mathf.FloorToInt(1280f * imageQuality);
        int height = Mathf.FloorToInt(720f * imageQuality);

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        screenshotCamera.targetTexture = rt;
        screenshotCamera.Render();

        RenderTexture.active = rt;
        Texture2D sceneScreenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        sceneScreenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        sceneScreenshot.Apply();

        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

#if UNITY_EDITOR
        string filePath = Path.Combine(savePath, $"{hullpartName}_{view}_Screenshot.png");
        byte[] bytes = sceneScreenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
#endif

        string base64String = Convert.ToBase64String(sceneScreenshot.EncodeToPNG());

        Destroy(sceneScreenshot);
        Destroy(rt);
        return base64String;
    }
}



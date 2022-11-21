using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class EncryptHelper
{

    private static readonly string AesIV = @"nttcom0033vrteam"; // AESで使用するIV 半角16文字
    private static readonly string AesKey = @"nttcommunications0033tlxteam0033"; // AESで使用するキー 半角32文字
    private static readonly int KeySize = 256;
    private static readonly int BlockSize = 128;


    /// <summary>
    /// エンコード
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Encrypt(string text)
    {

        // AESオブジェクトを取得します。
        var aes = GetAesManaged();

        // 対象の文字列をバイトデータに変換します。
        var byteValue = Encoding.UTF8.GetBytes(text);

        // バイトデータの長さを取得します。
        var byteLength = byteValue.Length;

        // 暗号化オブジェクトを取得します。
        var encryptor = aes.CreateEncryptor();

        // 暗号化します。
        var encryptValue = encryptor.TransformFinalBlock(byteValue, 0, byteLength);

        // 暗号化されたバイトデータをBase64文字列に変換します。
        var base64Value = Convert.ToBase64String(encryptValue);
        return base64Value;


    }



    /// <summary>
    /// デコード
    /// </summary>
    /// <param name="cipher"></param>
    /// <returns></returns>
    public static string Decrypt(string cipher)
    {

        // AESオブジェクトを取得します。
        var aes = GetAesManaged();

        // 暗号化されたBase64文字列をバイトデータに変換します。
        var byteValue = Convert.FromBase64String(cipher);

        // バイトデータの長さを取得します。
        var byteLength = byteValue.Length;

        // 復号化オブジェクトを取得します。
        var decryptor = aes.CreateDecryptor();

        // 復号化します。
        var decryptValue = decryptor.TransformFinalBlock(byteValue, 0, byteLength);

        // 復号化されたバイトデータを文字列に変換します。
        var stringValue = Encoding.UTF8.GetString(decryptValue);
        return stringValue;

    }

    
    private static AesManaged GetAesManaged()
    {
        // AESオブジェクトを生成し、パラメータを設定します。
        var aes = new AesManaged();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.IV = Encoding.UTF8.GetBytes(AesIV);
        aes.Key = Encoding.UTF8.GetBytes(AesKey);
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }
    

}

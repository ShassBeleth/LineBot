using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parrot.ConfigData;
using Parrot.Helper;

namespace Parrot.Controllers {

	/// <summary>
	/// LINE BotからのWebhook送信先API
	/// </summary>
	public class CallbackController : ApiController {
		
		/// <summary>
		/// Reply Messageに使用するリクエストEntity
		/// </summary>
		private class RequestOfReplyMessage {

			public class Message {

				public string type;

				public string text;

			}

			public string replyToken;

			public Message[] messages;

		}

		public HttpResponseMessage Post( JToken token ) {
			
			Trace.TraceInformation( "リクエスト情報\n" + token.ToString() );

			#region 入力チェックと通知から必要な情報の取得
			string replyToken = "";
			string sentMessage = "";
			{
				foreach( JToken events in token[ "events" ] ) {

					if( !"message".Equals( events[ "type" ].ToString() ) ) {
						Trace.TraceInformation( "通知以外によるAPI呼び出しのため反応しない" );
						return new HttpResponseMessage( HttpStatusCode.OK );
					}

					if( !"user".Equals( events[ "source" ][ "type" ].ToString() ) ) {
						Trace.TraceInformation( "個人での呼び出しでないため反応しない" );
						return new HttpResponseMessage( HttpStatusCode.OK );
					}

					if( !"text".Equals( events[ "message" ][ "type" ].ToString() ) ) {
						Trace.TraceInformation( "文字以外の通知には反応しない" );
						return new HttpResponseMessage( HttpStatusCode.OK );
					}

					Trace.TraceInformation( "ReplyToken is : " + events[ "replyToken" ].ToString() );
					replyToken = events[ "replyToken" ].ToString();

					Trace.TraceInformation( "Message is : " + events[ "message" ][ "text" ].ToString() );
					sentMessage = events[ "message" ][ "text" ].ToString();

				}
			}
			#endregion
			
			#region 通知に対するリプライを返す
			{

				#region リクエスト情報の作成
				StringContent content;
				{

					RequestOfReplyMessage requestObject = new RequestOfReplyMessage();
					requestObject.replyToken = replyToken;
					RequestOfReplyMessage.Message message = new RequestOfReplyMessage.Message();
					message.type = "text";
					message.text = this.CreateMessage( sentMessage );
					requestObject.messages = new RequestOfReplyMessage.Message[1];
					requestObject.messages[ 0 ] = message;

					string jsonRequest = JsonConvert.SerializeObject( requestObject );
					Trace.TraceInformation( "Reply Message Request is : " + jsonRequest );
					content = new StringContent( jsonRequest );
					content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

				}
				#endregion
				
				string result = AsyncHelper.RunSync<string>( () => this.PostReplyMessage( LineBotConfig.ChannelAccessToken , LineBotConfig.ReplyMessageUrl , content ) );
				Trace.TraceInformation( "Reply Message Result is : " + result );

			}
			#endregion
			
			//常にステータス200を返す
			return new HttpResponseMessage( HttpStatusCode.OK );
		}

		/// <summary>
		/// リプライメッセージを作成する
		/// </summary>
		/// <param name="sentMessage">送られてきたメッセージ</param>
		/// <returns>返信メッセージ</returns>
		private string CreateMessage( string sentMessage ) {
			return sentMessage + "クエー！";
		}

		/// <summary>
		/// Reply MessageをPOSTする
		/// </summary>
		/// <param name="token">Botを一意に決定するチャンネルアクセストークン</param>
		/// <param name="url">Reply Message APIのURL</param>
		/// <param name="content">通知情報</param>
		/// <returns></returns>
		private async Task<string> PostReplyMessage( string token , string url , StringContent content ) {

			try {

				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
				client.DefaultRequestHeaders.Add( "Authorization" , "Bearer {" + token + "}" );
				
				HttpResponseMessage response = await client.PostAsync( url , content ).ConfigureAwait( false );
				Trace.TraceInformation( "Reply Message Response Status is : " + response.StatusCode );
				return await response.Content.ReadAsStringAsync();
				
			}
			catch( Exception ex ) {
				Trace.TraceError( "Reply Message Error is : " + ex.Message );
				return null;
			}

		}

	}

	

}
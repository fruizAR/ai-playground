import { AppComponent } from "./app.component";
import { BrowserModule } from "@angular/platform-browser";
import { ChatComponent } from "./components/chat/chat.component";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { MarkdownModule } from "ngx-markdown";
import { NgModule } from "@angular/core";
import { OpenAIService } from "./services/openai.service";

@NgModule({
  declarations: [AppComponent, ChatComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    MarkdownModule.forRoot(),
  ],
  providers: [OpenAIService],
  bootstrap: [AppComponent],
})
export class AppModule {}
